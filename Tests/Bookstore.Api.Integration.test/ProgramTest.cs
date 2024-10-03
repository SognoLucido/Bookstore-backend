using Bookstore.Api.Integration.test.Dataseed;
using Bookstore_backend;
using Database.ApplicationDbcontext;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Data.Common;
using Testcontainers.PostgreSql;



namespace Bookstore.Api.Integration.test;


public class ProgramTestApplicationFactory : WebApplicationFactory<Program> , IAsyncLifetime
{

    private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder().Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {

        builder.ConfigureLogging(x=>x.ClearProviders());
        

        builder.ConfigureServices(services =>
        {
            var dbContext = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<Booksdbcontext>));

            services.Remove(dbContext);

            var dbConnection = services.SingleOrDefault(d => d.ServiceType == typeof(DbConnection));

            services.Remove(dbConnection);


            services.AddSingleton<DbConnection>(container =>
            {
                var connection = new  NpgsqlConnection(_postgreSqlContainer.GetConnectionString());
                connection.Open();

                return connection;
            });

            services.AddDbContext<Booksdbcontext>((container, options) =>
            {
                var connection = container.GetRequiredService<DbConnection>();
                options.UseNpgsql(connection);

            });

            
            services.AddSingleton<DataseedperTestLogic>();
           
        });



        builder.UseEnvironment("xunit");

    }



    Task IAsyncLifetime.InitializeAsync()
    {
        return _postgreSqlContainer.StartAsync();
    }

    Task IAsyncLifetime.DisposeAsync()
    {
        return _postgreSqlContainer.DisposeAsync().AsTask();
    }
}


