using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;
using Database.ApplicationDbcontext;
using Microsoft.Extensions.Configuration;

namespace Database
{
    internal class DiogelContextFactory : IDesignTimeDbContextFactory<Booksdbcontext>
    {
  
        public Booksdbcontext CreateDbContext(string[] args)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "..", "testpostgre"))
                .AddJsonFile("appsettings.json")
                .Build();

            DbContextOptionsBuilder<Booksdbcontext> dbContextOptionsBuilder = new();

            dbContextOptionsBuilder.UseNpgsql(configuration.GetConnectionString("Database"));
            return new Booksdbcontext(dbContextOptionsBuilder.Options);
        }
    }

}
