using Database.Model;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Reflection.Metadata;



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

       
      

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

           





            //modelBuilder.Entity<RolesModel>().HasData(
            //    new RolesModel
            //    {
            //        Id = 1,
            //        Roles = "admin"
            //    },
            //    new RolesModel
            //    {
            //        Id= 2,
            //        Roles = "user"
            //    }
                
            //    );


            ////admin login = user :admin@example.com, psswd: admin

            //modelBuilder.Entity<Customer>().HasData(
            //    new Customer
            //    {
            //        Id = Guid.NewGuid(),
            //        FirstName = "Admin",  
            //        LastName = "Admin",
            //        Email = "admin@example.com",
            //        Password = "7fb1cf92faf20c657c1fee16d6e975eb5c8b61a82cbaaf66a2c9a2c2c19addf1",
            //        Salt = "e1ed2b31",
            //        Phone = "yes331",
            //        RolesModelId = 1,


            //    });









            //modelBuilder.Entity<RolesModel>()
            // .HasMany(e => e.Customer)
            // .HasForeignKey(e => e.CustomersId);

            //modelBuilder.Entity<RolesAcUserManytomany>().HasKey(sc => new { sc.RoleModel, sc.CustomersId });

            //modelBuilder.Entity<RolesModel>()
            //    .HasNoKey()
            //    .HasOne(x => x.Customer)
            //    .WithOne()
            //    .HasForeignKey<Customer>(x => x.CustomerId)
            //    .HasPrincipalKey<RolesModel>(x => x.CustomerId);


            //modelBuilder.Entity<Customer>()
            //    .HasMany(c => c.Orders)

        }

    }

}
