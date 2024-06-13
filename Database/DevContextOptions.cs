using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;
using Database.ApplicationDbcontext;
using Microsoft.Extensions.Configuration;

namespace Database
{
   
    public class BooksContextFactory : IDesignTimeDbContextFactory<Booksdbcontext>
    {


        //public Booksdbcontext CreateDbContext(string[] args) 
        //{



        //    //IConfigurationRoot configuration = new ConfigurationBuilder()
        //    //    .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "..", "testpostgre"))
        //    //    .AddJsonFile("appsettings.json")
        //    //    .Build();



        //    var optionsBuilder = new DbContextOptionsBuilder<Booksdbcontext>() ;
        //    optionsBuilder.UseNpgsql("Server=localhost;Port=5432;Database=booksdb;Username=postgres;Password=changethis;");
        //    //optionsBuilder.UseNpgsql(configuration.GetConnectionString("database"));
        //    return new Booksdbcontext(optionsBuilder.Options);
        //}

        //public Booksdbcontext CreateDbContext(string[] args)

        //public BooksContextFactory()

        //development dotnet ef migrations only
        public Booksdbcontext CreateDbContext(string[] args)
        {

            //var a = Directory.GetCurrentDirectory();
            //var b = Path.Combine(a, "..");

            //var c = Path.Combine(b, "testpostgre");

            //Console.WriteLine(a);
            //Console.WriteLine(b);
            //Console.WriteLine(c);

           


            IConfigurationRoot configuration2 = new ConfigurationBuilder()
              .SetBasePath(Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(),"../Bookstore-backend")))
              .AddJsonFile("appsettings.json")
              .Build();

        //var z = configuration2.GetConnectionString("database");

        //D:\CODE\source\repos\Bookstore - backend\Bookstore - backend\appsettings.json

          



            var optionsBuilder = new DbContextOptionsBuilder<Booksdbcontext>();
            //optionsBuilder.UseNpgsql("Server=localhost;Port=5432;Database=booksdb;Username=postgres;Password=changethis;");
            optionsBuilder.UseNpgsql(configuration2.GetConnectionString("database"));
            //return new Booksdbcontext(optionsBuilder.Options);
            return new Booksdbcontext(optionsBuilder.Options);

        }






        //public Booksdbcontext CreateDbContext(string[] args)
        //{

        //    IConfigurationRoot configuration2 = new ConfigurationBuilder()
        //     .SetBasePath(Directory.GetCurrentDirectory())
        //     .AddJsonFile("appsettings.json")
        //     .Build();


        //    var optionsBuilder = new DbContextOptionsBuilder<Booksdbcontext>();
        //    optionsBuilder.UseNpgsql(configuration2.GetConnectionString("database"));
        //    return new Booksdbcontext(optionsBuilder.Options);
        //}










    }



}
