using Bookstore_backend;
using Database.ApplicationDbcontext;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
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
            var dbContextDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<Booksdbcontext>));

            services.Remove(dbContextDescriptor);

            var dbConnectionDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbConnection));

            services.Remove(dbConnectionDescriptor);

            

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
        });


        builder.UseEnvironment("Development");
    }


    public Task InitializeAsync()
    {
        return _postgreSqlContainer.StartAsync();
    }

    Task IAsyncLifetime.DisposeAsync()
    {
        return _postgreSqlContainer.DisposeAsync().AsTask();
    }

}




//namespace Bookstore.Api.Integration.test
//{
//    public class ProgramTest : WebApplicationFactory<IEntryPoint>/*, IAsyncLifetime*/
//    {
//        //public readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder().Build();

//        private readonly WebApplicationFactory<Program> _factory;

//        //public ProgramTests(WebApplicationFactory<Program> factory)
//        //{
//        //    _factory = factory;
//        //}
//        //protected override void ConfigureWebHost(IWebHostBuilder builder)
//        //{
//        //    builder.ConfigureLogging(x =>
//        //    {
//        //        x.ClearProviders();
//        //    });

//        //    //builder.ConfigureServices(services =>
//        //    //{
//        //    //    services.RemoveAll(typeof(IDbConnection));
//        //    //    services.AddSingleton<IDbConnection>(_ => new NpgsqlConnection(_postgreSqlContainer.GetConnectionString()));

//        //    //});

//        //    builder.ConfigureServices(services =>
//        //    {

//        //    });
//        //}

//        //public async Task InitializeAsync()
//        //{
//        //    _postgreSqlContainer.StartAsync();

//        //    // string test = _postgreSqlContainer.GetConnectionString();

//        //}

//        //public Task InitializeAsync()
//        //{
//        //    return _postgreSqlContainer.StartAsync();
//        //}

//        //Task IAsyncLifetime.DisposeAsync()
//        //{
//        //    return _postgreSqlContainer.DisposeAsync().AsTask();
//        //}
//    }
//}
