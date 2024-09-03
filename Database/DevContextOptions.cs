using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;
using Database.ApplicationDbcontext;
using Microsoft.Extensions.Configuration;

namespace Database
{
   
    public class BooksContextFactory : IDesignTimeDbContextFactory<Booksdbcontext>
    {


     
        //development dotnet ef migrations only
        public Booksdbcontext CreateDbContext(string[] args)
        {

            IConfigurationRoot configuration2 = new ConfigurationBuilder()
              .SetBasePath(Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(),"../Bookstore-backend")))
              .AddJsonFile("appsettings.json")
              .Build();


            var optionsBuilder = new DbContextOptionsBuilder<Booksdbcontext>();
            
            optionsBuilder.UseNpgsql(configuration2.GetConnectionString("database"));
          
            return new Booksdbcontext(optionsBuilder.Options);

        }



    }



}
