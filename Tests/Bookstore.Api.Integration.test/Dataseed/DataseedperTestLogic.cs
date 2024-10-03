using Database.ApplicationDbcontext;
using Database.DatabaseLogic;
using Database.Mapperdtotodb;
using Database.Model;
using Database.Model.ModelsDto;
using Microsoft.EntityFrameworkCore;


namespace Bookstore.Api.Integration.test.Dataseed
{
    public class DataseedperTestLogic(Booksdbcontext _context)
    {
        private readonly Booksdbcontext context = _context;

        public async Task CleanBooks()
        {
            await context.Database.ExecuteSqlRawAsync("DELETE FROM \"Books\"");
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


        public async Task InsertCustombook(BookinsertModel Custombook)
        {
            var BooktoInsert = Custombook.MapTobook();

            BooktoInsert.AuthorId = 1;
            BooktoInsert.CategoryId = 1;

            await context.Books.AddAsync(BooktoInsert);
            await context.SaveChangesAsync();
        }


        public async Task AuthDataSeedInit(Customer customer, string passw)
        {
            var hasher = new Passhasher();

            var (hashed, salt) = await hasher.HashpasstoDb(passw);

            customer.Password = hashed;
            customer.Salt = salt;

            customer.Apiservice = new Apiservice()
            {
                CustomerId = customer.Id,
                Apikey = Guid.NewGuid(),
                SubscriptionTier = Subscription.Tier2

            };



            //var check1 =context.Database.CanConnect();
            //var check2 =context.Database.EnsureCreated();



            await context.Customers.AddAsync(customer);
            await context.SaveChangesAsync();
        }






    }
}
