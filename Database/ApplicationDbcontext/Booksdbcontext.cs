using Database.Model;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;



namespace Database.ApplicationDbcontext
{
    public class Booksdbcontext : DbContext
    {

        public Booksdbcontext(DbContextOptions<Booksdbcontext> options) : base(options) { }

        public DbSet<Author> Authors { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        public DbSet<Person> Person { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {




            //modelBuilder.Entity<Student>()
            //    .HasOne<Grade>(s => s.Grade)
            //    .WithMany(g => g.Students)
            //    .HasForeignKey(s => s.CurrentGradeId);
        }

    }

}
