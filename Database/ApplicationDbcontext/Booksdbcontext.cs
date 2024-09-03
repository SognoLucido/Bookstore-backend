using Database.Model;
using Microsoft.EntityFrameworkCore;


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

        public DbSet<RolesModel> Roles { get; set; }

        public DbSet<Apiservice> Api { get; set; }
       
      

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {



            modelBuilder.Entity<RolesModel>().HasData(
                new RolesModel
                {
                    Id = 1,
                    Roles = "admin"
                },
                new RolesModel
                {
                    Id = 2,
                    Roles = "user"
                }

                );




            modelBuilder.Entity<Apiservice>()
                .HasKey(x => x.CustomerId)
                .HasName("UserID");

            modelBuilder.Entity<Apiservice>()
                .HasOne(x => x.Customer)
                .WithOne(x => x.Apiservice);




        }

    }

}
