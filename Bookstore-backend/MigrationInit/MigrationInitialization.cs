using Database.ApplicationDbcontext;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Text.Json;

namespace Bookstore_backend.MigrationInit
{
    public static class MigrationInitialization
    {
        public static async void ApplyMigration(this IApplicationBuilder app)
        {
            using IServiceScope scope = app.ApplicationServices.CreateScope();

            using Booksdbcontext dbContext = scope.ServiceProvider.GetRequiredService<Booksdbcontext>();


            Console.WriteLine($"database available : {dbContext.Database.CanConnect()}");
            //Console.WriteLine($"database CanConnect : {dbContext.Database.EnsureCreated()}");

            bool check = false;

            try
            {
                var migrations = dbContext.Database.GetAppliedMigrations();
                check = migrations.Any(m => m.Contains("BaseStart"));

            }
            catch (NpgsqlException ex) { Console.WriteLine(ex.Message); Environment.Exit(1); }
            catch (Exception ex) { Console.WriteLine(ex.Message); Environment.Exit(1); }


            if (check is false)
                try
                {
                   await dbContext.Database.MigrateAsync();

                    var des = new JsonDataseedParser();
                    var adminjsondata = des.AdminDeserialize() ?? throw new JsonException();
                    var booksjsondata =  des.BooksDeserialize() ?? throw new JsonException();

                    await dbContext.Customers.AddAsync(adminjsondata.Admindata);
                    await dbContext.Api.AddAsync(adminjsondata.Adminapidata);
                    await dbContext.Authors.AddRangeAsync(booksjsondata.authors);
                    await dbContext.Categories.AddRangeAsync(booksjsondata.categories);
                    await dbContext.Books.AddRangeAsync(booksjsondata.books);

                    await dbContext.SaveChangesAsync();

                }
                catch (JsonException ex) { Console.WriteLine($"Invalid Jsonfile , parse failed \n ERROR : {ex.Message}"); Environment.Exit(1); }
                catch (NpgsqlException ex) { Console.WriteLine(ex.Message); Environment.Exit(1); }
                catch (Exception ex) { Console.WriteLine(ex.Message); Environment.Exit(1); }





        }
    }
}
