﻿using Bookstore.Api.Integration.test.Model;
using Database.ApplicationDbcontext;
using Database.DatabaseLogic;
using Database.Mapperdtotodb;
using Database.Model;
using Database.Model.Apimodels;
using Database.Model.ModelsDto;
using Microsoft.EntityFrameworkCore;
using Subscription = Database.Model.Subscription;



namespace Bookstore.Api.Integration.test.Dataseed
{

   


    public class DataseedperTestLogic(Booksdbcontext _context)
    {
        private readonly Booksdbcontext context = _context;

        public async Task CleanBooks()
        {
            await context.Database.ExecuteSqlRawAsync("DELETE FROM \"Books\"");
        }

        public async Task CleanAuthorNCategory()
        {
            await context.Database.ExecuteSqlRawAsync(@" DELETE FROM ""Authors"";DELETE FROM ""Categories"";");
        }

        public async Task CleanUser()
        {
            await context.Database.ExecuteSqlRawAsync(@" DELETE FROM ""Api"";DELETE FROM ""Customers"";");
        }

        public async Task InsertCustomAuthorCategory(string AuthorName , string CategoryName)
        {
            await context.Authors.AddAsync(new Author { FullName = AuthorName.ToLower(), Bio = "string" });
            await context.Categories.AddAsync(new Category { Name = CategoryName.ToLower() });

            await context.SaveChangesAsync();
        }

        public async Task BaseDatabookseed()
        {
            var des = new JsonDataseedParsertest();
            var Booksdata = des.DataDeserialize();


            await context.Authors.AddRangeAsync(Booksdata.authors);
            await context.Categories.AddRangeAsync(Booksdata.categories);
            await context.SaveChangesAsync();

            await context.Books.AddRangeAsync(Booksdata.books);
            await context.SaveChangesAsync();

        }



        public async Task InsertCustombook(List<BookinsertModel> Custombook)
        {
            var Booklist = new List<Book>();

            foreach (var book in Custombook)
            {
                Booklist.Add(book.MapTobook());

            }

            foreach(var book in Booklist)
            {
                book.AuthorId = 1;
                book.CategoryId = 1;
            }

            await context.Books.AddRangeAsync(Booklist);
            await context.SaveChangesAsync();

        }


        /// <summary>
        /// This method Insert a book in the database .
        /// </summary>
        /// <remarks>
        /// Use this method carefully; the Author and Category IDs are hardcoded to ID 1 , 
        /// at least one Category and Author record must be created before calling this ;  call after BaseDatabookseed()
        /// </remarks>
        public async Task InsertCustombook(BookinsertModel Custombook)
        {
            var BooktoInsert = Custombook.MapTobook();

            BooktoInsert.AuthorId = 1;
            BooktoInsert.CategoryId = 1;

            await context.Books.AddAsync(BooktoInsert);
            await context.SaveChangesAsync();
        }


        public async Task AuthDataSeedInit(Customer customer, string passw, UserRole role = UserRole.user)
        {
            var hasher = new Passhasher();

            var (hashed, salt) = await hasher.HashpasstoDb(passw);

            customer.Password = hashed;
            customer.Salt = salt;

            customer.Apiservice = new Apiservice()
            {
                CustomerId = customer.Id,
                Apikey = Guid.NewGuid(),
                SubscriptionTier = role == UserRole.admin ? Subscription.Tier2 : Subscription.Tier0

            };


            await context.Customers.AddAsync(customer);
            await context.SaveChangesAsync();
        }

        public async Task InsertDummyuser(Login user, UserRole role = UserRole.user)
        {
            var Testuser = new Customer()
            {
                Id = Guid.NewGuid(),
                FirstName = role == UserRole.admin ? "Admin" : "Test",
                LastName = role == UserRole.admin ? "Admin" : "User",
                Email = user.Email,
                //password
                //salt
                Address = "home",
                Phone = "string",
                RolesModelId = (int)role


            };


            await AuthDataSeedInit(Testuser, user.Password, role);


        }


        public async Task<AuthorNCategoryOnlyNames> CheckAuthorNCategory()
        {

            return new AuthorNCategoryOnlyNames
            {
                Authors = await context.Authors.Select(x=>x.FullName).ToListAsync(),
                Category = await context.Categories.Select(x => x.Name).ToListAsync(),
            };

        }

        public async Task<int> Checkstockitemdb(string isbn) => await context.Books.Where(x => x.ISBN == isbn).Select(s => s.StockQuantity).FirstAsync();
       
        public async Task<Decimal> CheckBookPrice(string isbn) => await context.Books.Where(x=>x.ISBN == isbn).Select(s=>s.Price).FirstAsync();

        public async Task<bool> Checkbookexist(string isbn) => await context.Books.AnyAsync(x => x.ISBN == isbn);

        public async Task<bool> UserExist (string email) => await context.Customers.AnyAsync(x => x.Email == email);
    }
}
