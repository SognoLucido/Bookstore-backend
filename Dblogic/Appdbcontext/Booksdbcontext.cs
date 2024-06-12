using Dblogic.Models;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Dblogic.Applicationcontext;

public class AppdbContext : DbContext
{

    public AppdbContext(DbContextOptions<AppdbContext> options) : base(options) { }


    public DbSet<Person> People { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        
    }


}