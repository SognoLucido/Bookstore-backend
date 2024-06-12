using Database.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;


namespace Database.ApplicationDbcontext
{
    public class Booksdbcontext : DbContext
    {

        public Booksdbcontext(DbContextOptions<Booksdbcontext> options) : base(options) { }

       public DbSet<Person> People { get; set; }
    }

   





}
