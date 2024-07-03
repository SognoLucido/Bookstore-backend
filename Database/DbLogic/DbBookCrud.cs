using Database.ApplicationDbcontext;
using Database.Mapperdtotodb;
using Database.Model;
using Database.Model.Apimodels;
using Database.Model.ModelsDto;
using Database.Model.ModelsDto.Paymentmodels;
using Database.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;
using System.Linq;



namespace Database.DatabaseLogic;

public class DbBookCrud : ICrudlayer
{

    private readonly Booksdbcontext _context;
    private readonly IpassHash passHash;
    public DbBookCrud(Booksdbcontext context,IpassHash _passHash)
    {
        _context = context;
        passHash = _passHash;

    }


    public async Task<List<BooksCatalog>> RawReturn(int page,int pagesize, CancellationToken token = default)
    {

          return  await _context.Books
            .Where(x=> x.StockQuantity>0)
             .Join(_context.Authors,
              book => book.AuthorId,
              author => author.AuthorId,
              (book, author) => new { book, author })
            .Join(_context.Categories,
              book => book.book.CategoryId,
              category => category.CategoryId,
              (bookAuthor, category) => new BooksCatalog
              {
                  ISBN = bookAuthor.book.ISBN,
                  BookTitle = bookAuthor.book.Title,
                  Category = category.Name,
                  Price = bookAuthor.book.Price.ToString()+"$",
                  Description = bookAuthor.book.Description,
                  AuthorName = bookAuthor.author.FullName,


              }).Skip((page-1)*pagesize).Take(pagesize).AsNoTracking().ToListAsync(token);



    }


    public async Task<List<DetailedFilterBookModel>> Filteredquery(QuerySelector selector, CancellationToken token = default)
    {

        var query =  _context.Books
            .Where(x => x.StockQuantity > 0)
             .Join(_context.Authors,
              book => book.AuthorId,
              author => author.AuthorId,
              (book, author) => new { book, author })
            .Join(_context.Categories,
              book => book.book.CategoryId,
              category => category.CategoryId,
              (bookAuthor, category) => new {bookAuthor,category}).AsQueryable();


        
        if (!string.IsNullOrEmpty(selector.ByBooktitle))
        {
            query = query.Where(b => b.bookAuthor.book.Title == selector.ByBooktitle.ToLower());
        }
        if (!string.IsNullOrEmpty(selector.ByAuthor))
        {
            query = query.Where(b => b.bookAuthor.author.FullName == selector.ByAuthor.ToLower());
        }
        if (!string.IsNullOrEmpty(selector.ByCategory))
        {
            query = query.Where(b => b.category.Name == selector.ByCategory.ToLower());
        }


        var Bookquery = await query.Select(b => new DetailedFilterBookModel
        {
            BookTitle = b.bookAuthor.book.Title,
            Category = b.category.Name,
            Price = b.bookAuthor.book.Price.ToString() + "$",
            Description = b.bookAuthor.book.Description,
            AuthorName = b.bookAuthor.author.FullName,
            ISBN = b.bookAuthor.book.ISBN,
            PublicationDate = b.bookAuthor.book.PublicationDate


        }).AsNoTracking().ToListAsync(token);



        return Bookquery;
    }



    public async Task<MarketDataAPIModelbyISBN?> Getbyisbn(string isbn, CancellationToken token = default)
    {

      var x = await _context.Books.Where(a => a.ISBN == isbn)
            .Select(a => new MarketDataAPIModelbyISBN
            {
                ISBN = a.ISBN,
                Price = a.Price.ToString() + "$",
                StockQuantity = a.StockQuantity

            }).FirstOrDefaultAsync(token);

        return x;
       


    }

    public async Task<bool> Deletebyisbn(string isbn, CancellationToken token = default)
    {

        var x = await _context.Books.FirstOrDefaultAsync(a => a.ISBN == isbn);


        if (x is null) return false;
        else
        {
            _context.Books.Remove(x);
            await _context.SaveChangesAsync(token);
            return true;
        }



    }






    //WWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWW


    public async Task ConcurTest(int delay,int qnty)
    {
       

       
        await Task.Delay(TimeSpan.FromSeconds(delay));

        await _context.Books
               .Where(b => b.ISBN == "9783161484100")
               .ExecuteUpdateAsync(x => x.SetProperty(a => a.StockQuantity, a => a.StockQuantity + qnty));



        await _context.SaveChangesAsync();
       
    }

    public async Task<Respostebookapi> InsertBookItem(BookinsertModel datamodel)
    {
        bool force = false;
      

        var query = _context.Books
            .Where(x => x.StockQuantity > 0)
             .Join(_context.Authors,
              book => book.AuthorId,
              author => author.AuthorId,
              (book, author) => new { book, author })
            .Join(_context.Categories,
              book => book.book.CategoryId,
              category => category.CategoryId,
              (bookAuthor, category) => new { bookAuthor, category }).AsQueryable();



        var exist = await query.Where(x => x.bookAuthor.book.ISBN == datamodel.ISBN).AsNoTracking().AnyAsync();

        if (!exist)
        {
            return new Respostebookapi()
            {
                Code = 409,
                Message = "the book already exist"
            };
        }

        if (!force)
        {
            var getAuthorId =  await _context.Authors.Where(a=>a.FullName == datamodel.Author ).AsNoTracking().FirstOrDefaultAsync();
            var getcategoryId = await _context.Categories.Where(a => a.Name == datamodel.Category).AsNoTracking().FirstOrDefaultAsync();


            var message = new Respostebookapi();
  

            if (getAuthorId  is null)
            {
                message.Code = 404;
                message.Message = " Author name not Found \n";

                
            }
            if(getcategoryId is null)
            {
                message.Code = 404;
                message.Message += " Author name not Found";

                
            }

            if(getAuthorId is null || getcategoryId is null)return message;

            if (getAuthorId is not null && getcategoryId is not null)
            {

                var modeltoinsert = datamodel.MapTobook();

                modeltoinsert.AuthorId = getAuthorId.AuthorId;
                modeltoinsert.CategoryId = getcategoryId.CategoryId;


                await _context.Books.AddAsync(modeltoinsert);


                try { 

                await _context.SaveChangesAsync();

                    message.Code = 200;
                    message.Message = "Sucessful inserted";
      
                    return message;

                }
                catch (Exception ex)
                {
                    message.Code = 500;
                    message.Message = ex.Message;

                    return message;
                }



                //await 


            }



        }
        


        //var y = await query
        //    .Where(x => x.bookAuthor.author.FullName == datamodel.Author || x.category.Name == datamodel.Category)
           
        //    .FirstOrDefaultAsync();



        // _context.Books.Add(datamodel);
        //var y =  await _context.SaveChangesAsync();



        return new Respostebookapi();
       


    }


    public async Task Testapi()
    {

        var x = new Book
        {
            Title = "test",
            AuthorId = 1,
            CategoryId = 1,
            ISBN = "9783161484199",
            Price = 5.5M,
            StockQuantity = 30,
            PublicationDate = new DateOnly(2010, 12, 12),
            Description = "test",
        };


        _context.Books.Add(x);
        await _context.SaveChangesAsync();


    }


    //pkkkkkkkkkkkkkkkkkkkkkkkk




    public async Task<bool> AddOrOverrideStockQuantitybyISBN(string isbn, int qnty, bool Forcerewrite , CancellationToken token = default)
    {
        
         if( Forcerewrite)
        {
            await _context.Books
               .Where(b => b.ISBN == isbn)
               .ExecuteUpdateAsync(x => x.SetProperty(a => a.StockQuantity, qnty), token);

        }
        else 
        {

            await _context.Books
                .Where(b => b.ISBN == isbn)
                .ExecuteUpdateAsync(x => x.SetProperty(a => a.StockQuantity, a => a.StockQuantity + qnty), token);

        }


 
        return true;
    }




    public async Task<bool> Registration(Registration regi,CancellationToken token = default)
    {

       var Findemailexist = await _context.Customers.Where(a => a.Email == regi.Email.ToLower()).FirstOrDefaultAsync(token);

        if(Findemailexist is not null ) return false;

        // procedo con la registrazione


        //creo il modello base da inserire nel database
        var modeltoinsert = regi.MaptoCustomer();

        //genero hash + salt dalla password dell'api model
        var( hash, salt) = await passHash.HashpasstoDb(regi.Password);
       
        //completo il modello db 
        modeltoinsert.Password= hash;
        modeltoinsert.Salt = salt;



        // procedo all'inserimento sicuro
        using IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(token);

        
            try
            {

                await _context.Customers.AddAsync(modeltoinsert, token);

                await _context.SaveChangesAsync(token);

                await transaction.CommitAsync(token);

                return true;
            }
            catch (Exception)
            {

                await transaction.RollbackAsync(token);
                return false;
            }

        
    }





    public async Task<PaymentDetails?> GetInvoicebooks(List<Model.ModelsDto.PaymentPartialmodels.BookItemList> bookitems)
    {

        var UserGuid = Guid.Parse("aad38dfe-1275-48d2-8793-3027caa50c09");


        var isbnANDqnty = bookitems.Select(b => (b.ISBN,b.Quantity)).Where(a=> a.Quantity>0 ).ToList();

        if(isbnANDqnty.Count != bookitems.Count)return null;

        //dinstict and merge

        var listisbn = isbnANDqnty.Select(a => a.ISBN);


        //var Getbooksprices = await _context.Books
        //    //.Where(a => isbns.Contains(a.ISBN) && a.StockQuantity > 0 )
        //    //.Where(a => isbns.Contains(a.ISBN) && a.StockQuantity > 0)
        //    .Select(x => new {
        //        x.ISBN,
        //        x.Title , 
        //        x.Price ,

        //    })
        //    .ToListAsync();


        //var Getbooksprices = _context.Books

        //    .Where(a => isbns.Contains(a.ISBN) && a.StockQuantity > 0)
        //    .Select(x => new
        //    {
        //        x.ISBN,
        //        x.Title,
        //        x.Price,

        //    }).AsQueryable();


        var GetdataMatchfromdb =  await _context.Books.Where(b => listisbn.Contains(b.ISBN))
            //.Select(x => new
            //{
            //    x.BookId,
            //    x.ISBN,
            //    x.Title,
            //    x.Price,
            //    x.StockQuantity,


            //})
            //.AsNoTracking()
            .ToListAsync();


           // stock Check 
        var Qntycheck = GetdataMatchfromdb.Where(a => isbnANDqnty
                    .Any(b => b.ISBN == a.ISBN && b.Quantity <= a.StockQuantity))
                .ToList();
         
        if (Qntycheck.Count != GetdataMatchfromdb.Count || Qntycheck.Count == 0) return null;


        using (var transaction = await _context.Database.BeginTransactionAsync()) 
        {

            try
            {

                foreach (var item in GetdataMatchfromdb) 
                {
                    item.StockQuantity -= isbnANDqnty.Where(a => a.ISBN == item.ISBN).First().Quantity;
                }

                await _context.SaveChangesAsync();


                var Orderinsert = new Order()
                {
                    CustomerId = UserGuid,
                    status = Status.Pending,
                    OrderDate = DateTime.UtcNow,
                };

               await _context.Orders.AddAsync(Orderinsert);

                await _context.SaveChangesAsync();





                var InsertOrderItems = new List<OrderItem>();


                foreach (var (ISBN, Quantity) in isbnANDqnty)
                {
                    var book = GetdataMatchfromdb.Where(a => a.ISBN == ISBN).FirstOrDefault();

                    InsertOrderItems.Add(new OrderItem
                    {
                        OrderId = Orderinsert.OrderId,
                        BookId = book!.BookId,
                        Quantity = Quantity,
                        Price = book.Price,
                    });


                    //InsertOrderItems.Add(new OrderItem
                    //{
                    //    OrderId = order.OrderId,
                    //    BookId = Getdatafromdb.Where(x => x.ISBN == ISBN).Select(x => x.BookId).First(),
                    //    Quantity = Quantity,
                    //    Price = Getdatafromdb.Where(x => x.ISBN == ISBN).Select(x => x.Price).First(),
                    //});
                }


                _context.OrderItems.AddRange(InsertOrderItems);

                await _context.SaveChangesAsync();

                //await _context.Customers.AddAsync(modeltoinsert, token);

                //await _context.SaveChangesAsync(token);




                await transaction.CommitAsync();

                //return true;
            }
            catch (Exception)
            {

                await transaction.RollbackAsync();
               return null; 
            }

        } ;




        //var GetBooksprices = _context.Books.AsQueryable();

        //foreach (var item in isbnANDqnty)
        //{
        //    GetBooksprices = GetBooksprices.TakeWhile(a => a.ISBN.Contains(item.ISBN) && item.Quantity <= a.StockQuantity);
        //}

        //var Getdatafromdb = await GetBooksprices
        //    .Select(x => new
        //    {
        //        x.BookId,
        //        x.ISBN,
        //        x.Title,
        //        x.Price,

        //    })
        //    .AsNoTracking()
        //    .ToListAsync();


        //if (isbnANDqnty.Count != Getdatafromdb.Count)return null;




        //var insertOrderToPending = ne








        var data = new PaymentDetails();




        data.TotalAmount = GetdataMatchfromdb.Sum(x => x.Price);

        foreach (var item in GetdataMatchfromdb)
        {
            data.Invoce.Add(new Invoice(item.Title, item.ISBN, item.Price));
        }




        return data.TotalAmount > 0 ? data : null;
    }









    public async Task<(string?,string?)> Login(Login login, CancellationToken token = default)
    {
       


        //var Getthepassw = await _context.Customers
        //    .FirstOrDefaultAsync(x => x.Email == login.Email,token);

        var Getthepassw =  await  _context.Customers
            .Where(x => x.Email == login.Email)
            .Join(_context.Roles,
              customer => customer.RolesModelId,
              roles => roles.Id,
              (customer, roles) =>  new
            {
                customer.Id,
                customer.Salt,
                customer.Password,
                roles.Roles

            }).FirstOrDefaultAsync(token);




        if (Getthepassw is not null)
        {
            if (await passHash.HashAlgorithm(login.Password, Getthepassw.Salt) == Getthepassw.Password) return (Getthepassw.Id.ToString(),Getthepassw.Roles);
            else return (null,null);
        }
        else return (null,null);



       
    }

   
    
}
