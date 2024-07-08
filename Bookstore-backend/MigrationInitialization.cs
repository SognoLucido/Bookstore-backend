using Database.ApplicationDbcontext;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Bookstore_backend
{
    public static class MigrationInitialization
    {
        public static void ApplayMigration(this IApplicationBuilder app)
        {
            using IServiceScope scope = app.ApplicationServices.CreateScope();

            using Booksdbcontext dbContext = scope.ServiceProvider.GetRequiredService<Booksdbcontext>();


            //try
            //{
            //    dbContext.Database.Migrate();
            //}
            //catch (NpgsqlException ex)
            //{
            //    Console.WriteLine("Database offline");
            //    Console.WriteLine(ex.Message);
            //    Environment.Exit(1);
            //}
            //catch(Exception ex)
            //{
            //    Console.WriteLine(ex.Message);
            //    Environment.Exit(1);
            //}

          

        }
    }
}
