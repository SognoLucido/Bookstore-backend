using Database.ApplicationDbcontext;
using Database.DatabaseLogic;
using Database.Services;
using Microsoft.EntityFrameworkCore;


namespace Bookstore_backend
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

         

            builder.Services.AddControllers();
  
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();


           
                builder.Services.AddDbContext<Booksdbcontext>(opt =>
                     opt.UseNpgsql(builder.Configuration.GetConnectionString("database")));

            //test
            //builder.Services.AddSingleton<BooksContextFactory>();
            //

            builder.Services.AddScoped<ICrudlayer, DbBookCrud>();


            var app = builder.Build();

           
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

           app.ApplayMigration();


            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
