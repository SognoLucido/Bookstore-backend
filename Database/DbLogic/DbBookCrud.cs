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



namespace Database.DatabaseLogic;

public class DbBookCrud : ICrudlayer
{

    private readonly Booksdbcontext _context;
    private readonly IpassHash passHash;
    public DbBookCrud(Booksdbcontext context, IpassHash _passHash)
    {
        _context = context;
        passHash = _passHash;

    }


    public async Task<List<BooksCatalog>> RawReturn(int page, int pagesize, CancellationToken token = default)
    {

        return await _context.Books
          .Where(x => x.StockQuantity > 0)
          .OrderBy(x => x.BookId)
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
                Price = bookAuthor.book.Price.ToString() + "$",
                Description = bookAuthor.book.Description,
                AuthorName = bookAuthor.author.FullName,


            })
          .Skip((page - 1) * pagesize)
          .Take(pagesize)
          .AsNoTracking()
          .ToListAsync(token);



    }


    public async Task<List<DetailedFilterBookModel>> Filteredquery(QuerySelector selector, CancellationToken token = default)
    {

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





    public async Task<(MarketDataAPIModelbyISBN?, Respostebookapi?)> ApiServiceGetbyisbn(string isbn, Guid apikey, CancellationToken token = default)
    {




        var checkkey = await _context.Api
            .Where(a => a.Apikey == apikey)
            .Select(b => new
            {
                b.SubscriptionTier,
                b.DateTime,
                b.Calls,
            })
            .AsNoTracking()
           .FirstOrDefaultAsync(token);

        if (checkkey == null) return (null, new Respostebookapi
        {
            Code = 404,
        });


        var dbresult = _context.Api.Where(a => a.Apikey == apikey).AsQueryable();

        bool CallscapCheck = false; // True if the cap has been reached




        bool Datecheck = checkkey.DateTime is null || (DateTime.UtcNow - checkkey.DateTime) >= TimeSpan.FromMinutes(3); // 3 min(hardcoded) // 


        switch (checkkey.SubscriptionTier)
        {
            case Subscription.Tier0: CallscapCheck = checkkey.Calls >= 5; break;
            case Subscription.Tier1: CallscapCheck = checkkey.Calls >= 10; break;

        }

        if (checkkey.SubscriptionTier == Subscription.Tier2)
        {
            await dbresult.ExecuteUpdateAsync(p => p.SetProperty(a => a.Calls, a => a.Calls + 1), token);
        }
        else
        {

            if (Datecheck)
            {
                var result = await dbresult.ExecuteUpdateAsync(p => p
                 .SetProperty(a => a.Calls, 1)
                 .SetProperty(a => a.DateTime, DateTime.UtcNow), token);
            }
            else if (!Datecheck && !CallscapCheck)
            {
                await dbresult.ExecuteUpdateAsync(p => p.SetProperty(a => a.Calls, a => a.Calls + 1), token);
            }
            else if (!Datecheck)
            {
                var TimetoReset = TimeSpan.FromMinutes(3) - (DateTime.UtcNow - checkkey.DateTime);





                return (null, new Respostebookapi
                {
                    Code = 429,
                    Message = $"You have exceeded your request limit. Please try again later in {TimetoReset!.Value:hh\\:mm\\:ss}"
                });

            }

        }

        var bookdata = await _context.Books.Where(a => a.ISBN == isbn)
              .Select(a => new MarketDataAPIModelbyISBN
              {
                  ISBN = a.ISBN,
                  Price = a.Price.ToString() + "$",
                  StockQuantity = a.StockQuantity

              }).AsNoTracking().FirstOrDefaultAsync(token);


     

        if(bookdata is null )
            return (null, new Respostebookapi
            {
                Code = 404,
                Message = $"book not found"
            });

        return (bookdata, null);
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




    public async Task<(List<DetailedFilterBookModel>?, Respostebookapi?)> InsertBooksItem(List<BookinsertModel> datamodel) 
    {


        var message = new Respostebookapi();


        var Bookfulllist = datamodel.MapTobookList();

        var BooksFreecloneList = Bookfulllist.DistinctBy(a => a.ISBN);

        var Booklistisbn = BooksFreecloneList.Select(x=>x.ISBN).ToList();

        var FilteredList = await  _context.Books
            .Where(x=> Booklistisbn.Contains(x.ISBN))
            .Select(x => x.ISBN)
            .ToListAsync();


        //if (FilteredList.Count > 0)
        var Booklist = BooksFreecloneList.Where(x => !FilteredList.Contains(x.ISBN)).ToList();

        foreach(var book in Booklist)
        {
            book.Author.FullName = book.Author.FullName.ToLower();
            book.Category.Name = book.Category.Name.ToLower();
        }
        
        var Authorlist = Booklist.DistinctBy(d=>d.Author.FullName).Select(a=>a.Author.FullName.ToLower());
        var Categorylist = Booklist.DistinctBy(d => d.Category.Name).Select(a => a.Category.Name.ToLower());

        var Getauthor = _context.Authors
             .Where(x => Authorlist.Contains(x.FullName))
             .Select(c => new
             {
                 c.AuthorId,
                 c.FullName,
             })
             .AsNoTracking()
             .ToDictionary(x=>x.FullName,v=>v.AuthorId);

        var GetCategory = _context.Categories
             .Where(x => Categorylist.Contains(x.Name))
             .Select(c => new
             {
                 c.CategoryId,
                 c.Name,
             })
             .AsNoTracking()
             .ToDictionary(x => x.Name, v => v.CategoryId);



        List<DetailedFilterBookModel> BookdataToReturn = [];


        for (int i = Booklist.Count - 1; i >= 0; i--)
        {

            if (Getauthor.TryGetValue(Booklist[i].Author.FullName, out int _authorid))
            {
                Booklist[i].AuthorId = _authorid;
            }
            else
            {
                Booklist.RemoveAt(i);
                continue;
            }

            if (GetCategory.TryGetValue(Booklist[i].Category.Name, out int _categoryid))
            {
                Booklist[i].CategoryId = _categoryid;
            }
            else
            {
                Booklist.RemoveAt(i);
                continue;
            }



            BookdataToReturn.Add(Booklist[i].DbbookmodelToApireturnBookmodel());

            Booklist[i].Author = default!;
            Booklist[i].Category = default!;

        }

        if (Booklist.Count is 0) return (null, new Respostebookapi()
        {
            Code = 400,
            Message = "Invalid records: Nothing to add"
        });

        try 
        { 

             await _context.Books.AddRangeAsync(Booklist);
             await _context.SaveChangesAsync();


        }
        catch(Exception ex)
        {
            Console.WriteLine("Failed to insert a range of books (api/book) into the db \n" + ex.Message);
            return (null,new Respostebookapi()
            {
                Code = 500,
                Message = "Failed insertion"
            });
        }





        return (BookdataToReturn, null);

    }












    public async Task<bool> AddOrOverrideStockQuantitybyISBN(string isbn, int qnty, bool Forcerewrite, CancellationToken token = default)
    {

        if (Forcerewrite)
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




    public async Task<bool> Registration(Registration regi, CancellationToken token = default)
    {

        var Findemailexist = await _context.Customers.Where(a => a.Email == regi.Email.ToLower()).FirstOrDefaultAsync(token);

        if (Findemailexist is not null) return false;

        // procedo con la registrazione


        //creo il modello base da inserire nel database
        var modeltoinsert = regi.MaptoCustomer();

        //genero hash + salt dalla password dall'api model
        var (hash, salt) = await passHash.HashpasstoDb(regi.Password);

        //completo il modello db 
        modeltoinsert.Password = hash;
        modeltoinsert.Salt = salt;



        // procedo all'inserimento sicuro
        using IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(token);


        try
        {

            var UserID = await _context.Customers.AddAsync(modeltoinsert, token);

            await _context.Api.AddAsync(new Apiservice
            {
                CustomerId = UserID.Entity.Id,
                Apikey = Guid.NewGuid(),

            }, token);

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





    public async Task<(PaymentDetails?, int?)> GetInvoicebooks(List<Model.ModelsDto.PaymentPartialmodels.BookItemList> bookitems, Guid UserGuidID)
    {

        //var UserGuid = Guid.Parse("aad38dfe-1275-48d2-8793-3027caa50c09");

        int OrderID;


        var Booklistfiltered = bookitems
              .Where(w => w.Quantity > 0)
              .GroupBy(x => x.ISBN)
              .ToDictionary(k => k.Key, k => k.Sum(t => t.Quantity));



        if (Booklistfiltered.Count == 0) return (null, null);


        var GetdataMatchfromdb = await _context.Books.Where(w => Booklistfiltered.Keys.Contains(w.ISBN)).ToListAsync();


        // stock Check 
        var Qntycheck = GetdataMatchfromdb.Where(a => Booklistfiltered
                    .Any(b => b.Key == a.ISBN && b.Value <= a.StockQuantity))
                    .ToList();

        if (Qntycheck.Count != GetdataMatchfromdb.Count || Qntycheck.Count == 0) return (null, null);



        var data = new PaymentDetails();





        using (var transaction = await _context.Database.BeginTransactionAsync())
        {

            try
            {

                foreach (var item in GetdataMatchfromdb)
                {
                    //item.StockQuantity -= Booklistfiltered.Where(a => a.Key == item.ISBN).First().Value;
                    item.StockQuantity -= Booklistfiltered[item.ISBN];
                }

                await _context.SaveChangesAsync();


                var Orderinsert = new Order()
                {
                    CustomerId = UserGuidID,
                    status = Status.Pending,
                    OrderDate = DateTime.UtcNow,
                };

                await _context.Orders.AddAsync(Orderinsert);

                await _context.SaveChangesAsync();





                var InsertOrderItems = new List<OrderItem>();


                foreach (var item in Booklistfiltered)
                {
                    var book = GetdataMatchfromdb.Where(a => a.ISBN == item.Key).FirstOrDefault();

                    InsertOrderItems.Add(new OrderItem
                    {
                        OrderId = Orderinsert.OrderId,
                        BookId = book!.BookId,
                        Quantity = item.Value,
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

                OrderID = Orderinsert.OrderId;


                data.TotalAmount = InsertOrderItems.Sum(a => a.Price * a.Quantity);

                //return true;
            }
            catch (Exception)
            {

                await transaction.RollbackAsync();
                return (null, null);
            }




        };




        //var data = new PaymentDetails
        //{
        //    TotalAmount = GetdataMatchfromdb.Sum(x => x.Price )
        //};

        foreach (var item in GetdataMatchfromdb)
        {
            data.Invoce.Add(new Invoice(item.Title, item.ISBN, item.Price, Booklistfiltered[item.ISBN]));
        }




        return data.TotalAmount > 0 ? (data, OrderID) : (null, null);
    }









    public async Task<(string?, string?)> Login(Login login, CancellationToken token = default)
    {



        //var Getthepassw = await _context.Customers
        //    .FirstOrDefaultAsync(x => x.Email == login.Email,token);

        var Getthepassw = await _context.Customers
            .Where(x => x.Email == login.Email)
            .Join(_context.Roles,
              customer => customer.RolesModelId,
              roles => roles.Id,
              (customer, roles) => new
              {
                  customer.Id,
                  customer.Salt,
                  customer.Password,
                  roles.Roles

              }).FirstOrDefaultAsync(token);




        if (Getthepassw is not null)
        {
            if (await passHash.HashAlgorithm(login.Password, Getthepassw.Salt) == Getthepassw.Password) return (Getthepassw.Id.ToString(), Getthepassw.Roles);
            else return (null, null);
        }
        else return (null, null);




    }







    public async Task UpdateOrderStatus(int OrderID, Status Ordertatus)
    {


        //////////

        await _context.Orders
               .Where(b => b.OrderId == OrderID)
               .ExecuteUpdateAsync(x => x.SetProperty(a => a.status, Ordertatus));
    }






    public async Task<bool> ChangeRoles(Guid? UserID, string? email, UserRole role)
    {


        bool check = false;



        var x = await _context.Customers
             .Where(b => b.Id == UserID || b.Email == email)
             .ExecuteUpdateAsync(p => p.SetProperty(a => a.RolesModelId, (int)role));


        if (x == 0) return false;

        if (UserID is not null)
        {
            var insert = _context.Api
                    .Where(a => a.CustomerId == UserID).AsQueryable();

            int check2 = 0;

            if (role == UserRole.admin)
            {
                check2 = await insert.ExecuteUpdateAsync(p => p.SetProperty(s => s.SubscriptionTier, Subscription.Tier2));
            }
            else if (role == UserRole.user)
            {
                check2 = await insert.ExecuteUpdateAsync(p => p.SetProperty(s => s.SubscriptionTier, Subscription.Tier0));
            }

            if (check2 == 0) return false;

        }
        else if (email is not null)
        {

            var emailtoUserID = _context.Customers
                .Where(a => a.Email == email)
                .Select(b => b.Id).FirstOrDefault();

            if (emailtoUserID == null) return false;



            var insert = _context.Api
                   .Where(a => a.CustomerId == emailtoUserID).AsQueryable();

            int check2 = 0;

            if (role == UserRole.admin)
            {
                check2 = await insert.ExecuteUpdateAsync(p => p.SetProperty(s => s.SubscriptionTier, Subscription.Tier2));
            }
            else if (role == UserRole.user)
            {
                check2 = await insert.ExecuteUpdateAsync(p => p.SetProperty(s => s.SubscriptionTier, Subscription.Tier0));
            }

            if (check2 == 0) return false;

        }

        return true;
    }




    //public async Task<bool> DeleteAccount(Guid? userid)
    //{

    //    var success =   await _context.Customers
    //       .Where(b => b.Id == userid )
    //       .ExecuteDeleteAsync();


    //    return success == 1 ;
    //}



    public async Task<bool> DeleteAccount<T>(T value)
    {


        var test = _context.Customers.AsQueryable();


        if (value is string Email)
        {
            test = test.Where(b => b.Email == Email && (b.RolesModelId != (int)UserRole.admin));
        }
        else if (value is Guid userID)
        {
            test = test.Where(b => b.Id == userID && b.RolesModelId != (int)UserRole.admin);
        }

        var result = await test.ExecuteDeleteAsync();

        return result == 1;
    }




    public async Task<bool> Pricebookset(string isbn, decimal price, CancellationToken token = default)
    {

        var result = await _context.Books
            .Where(a => a.ISBN == isbn)
            .ExecuteUpdateAsync(p => p.SetProperty(b => b.Price, price), token);

        return result == 1;
    }




    public async Task<bool> UpdateTier(Guid userID, Subscription subtier, HttpClient? client)
    {


        //var checkexist = await _context.Api.AnyAsync(x=>x.CustomerId == userID);


        using (var transaction = await _context.Database.BeginTransactionAsync())
        {



            try
            {
                var result = await _context.Api
               .Where(a => a.CustomerId == userID)
               .ExecuteUpdateAsync(p => p.SetProperty(s => s.SubscriptionTier, subtier));


                if (result == 0) return false;


                //await client.GetAsync("https://api.example.com/dbrecordsuccessfulsaved"); // external service to handle subscriptions



            }
            catch (HttpRequestException externalcallex)
            {

                await transaction.RollbackAsync();
                //do others stuff / rtry + count + log
                //Console.WriteLine(externalcallex.Message);
                return false;
            }
            catch (Exception Dbex)
            {
                await transaction.RollbackAsync();
                // call payment endpoint service to revert 
                //.. //await _httpClient.PostAsJsonAsync("https://api.example.com/paymentportalrevertstate",userID); // a mess lol
                return false;
            }


            await transaction.CommitAsync();

            return true;



        }



    }


    public async Task<UserInfo?> GetUserInfoAccount(Guid UserID)
    {

        //var GetfromuserTable = await _context.Customers
        //   .Where(x => x.Id == UserID)
        //   .Join(_context.Roles,
        //     customer => customer.RolesModelId,
        //     roles => roles.Id,
        //     (customer, roles) => new
        //     {
        //         customer.FirstName,
        //         customer.LastName,
        //         customer.Email,
        //         customer.Phone,

        //         roles.Roles

        //     }).AsNoTracking().FirstOrDefaultAsync();



        //var GetapiInfo = await _context.Customers
        //    .Where(x => x.Id == UserID)
        //    .Join(_context.Api,
        //    customer => customer.Id,
        //    apitable => apitable.CustomerId,
        //    (customer, apitable) => new
        //    {

        //    }).AsNoTracking().FirstOrDefaultAsync();


        //var GetfromuserTableTest2 = await _context.Customers
        //  .Where(x => x.Id == UserID).AsNoTracking().FirstOrDefaultAsync();



        var GetfromuserTableTest1 = await _context.Customers
          .Where(x => x.Id == UserID)
          .Join(_context.Roles,
            customer => customer.RolesModelId,
            roles => roles.Id,
            (customer, roles) => new { customer, roles })
          .Join(_context.Api,
            customer => customer.customer.Id,
            apitable => apitable.CustomerId,
            (customer, apitable) => new UserInfo
                (
                customer.customer.FirstName,
                customer.customer.LastName,
                customer.customer.Email,
                customer.customer.Phone,
                customer.roles.Roles,
                new Apiinfo
                    (
                    apitable.Apikey.ToString("N"),
                    //Tier: Enumconverter.EnumTostring(apitable.SubscriptionTier),
                    apitable.SubscriptionTier.ToString(),
                    apitable.Calls
                    )

                )
            )
            .AsNoTracking().FirstOrDefaultAsync();



        return GetfromuserTableTest1;

    }



    public async Task<List<AuthorDto>> GetAuthorinfo(int? limit, string? search)
    {
        //var data = _context.Authors.AsQueryable();

        //if (limit is not null) data = data.Take((int)limit);

        //return await data

        // .AsNoTracking()
        // .ToListAsync();


        var data = _context.Authors.AsQueryable();

        if (limit is not null) data = data.Take((int)limit);
        if (search is not null) data = data.Where(b => EF.Functions.Like(b.FullName, $"%{search}%"));

        return await data
            .Select(a => new AuthorDto
            {
                AuthorId = a.AuthorId,
                FullName = a.FullName,
                Bio = a.Bio,
                Books = _context.Books
                    .Where(b => b.AuthorId == a.AuthorId)
                    .Select(s => new Books
                    {
                        book = s.Title,
                    }).ToList()

            }).AsNoTracking().ToListAsync();



    }

    public async Task<List<Category>> GetCategoriesinfo(int? limit, string? search)
    {

        var data = _context.Categories.AsQueryable();

        if (search is not null) data = data.Where(b => EF.Functions.Like(b.Name, $"%{search}%"));
        if (limit is not null) data = data.Take((int)limit);
        return await data

         .OrderBy(o => o.Name)
         .AsNoTracking()
         .ToListAsync();


    }


    public async Task<List<DetailedFilterBookModel>> SearchItems(int? limit, (string? Booktitle, string? Authorname, string? Category) Tupledata, CancellationToken ctoken = default)
    {


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


        if (Tupledata.Booktitle is not null) query = query.Where(b => EF.Functions.Like(b.bookAuthor.book.Title, $"%{Tupledata.Booktitle.ToLower()}%"));
        if (Tupledata.Authorname is not null) query = query.Where(b => EF.Functions.Like(b.bookAuthor.author.FullName, $"%{Tupledata.Authorname.ToLower()}%"));
        if (Tupledata.Category is not null) query = query.Where(b => EF.Functions.Like(b.category.Name, $"%{Tupledata.Category.ToLower()}%"));

        if (limit is not null) query = query.Take((int)limit);


        var test = await query
            .OrderBy(b => b.bookAuthor.book.Title)
            .Select(b => new DetailedFilterBookModel
            {
                BookTitle = b.bookAuthor.book.Title,
                Category = b.category.Name,
                Price = b.bookAuthor.book.Price.ToString() + "$",
                Description = b.bookAuthor.book.Description,
                AuthorName = b.bookAuthor.author.FullName,
                ISBN = b.bookAuthor.book.ISBN,
                PublicationDate = b.bookAuthor.book.PublicationDate
            }).AsNoTracking().ToListAsync(ctoken);



        return test;

    }





    public async Task<bool> UpinsertAuthorsxCategories(CategoryandAuthorDto data, bool AuthorUpinsert)
    {

       



        if (data.Author is not null)
        {

            var filteredDataList = data.Author
                .DistinctBy(a => a.FullName.ToLower())
                .Select(x => new
                {
                    x.FullName,
                    x.Bio

                }).ToDictionary(z => z.FullName, z => z.Bio);


            var KeystoRemove = _context.Authors
                .Where(a => filteredDataList.Keys.Contains(a.FullName))
                .Select(p => new { p.AuthorId, p.FullName })
                .ToDictionary(x => x.FullName, x => x.AuthorId);

            if (AuthorUpinsert is false)
                foreach (var key in KeystoRemove)
                {
                    filteredDataList.Remove(key.Key);
                }

            if (filteredDataList.Count > 0)
            {


              //  var AuthortoInsert = new List<Author>();


                foreach (var author in filteredDataList)
                {
                    //AuthortoInsert.Add(new Author { Bio = author.Value, FullName = author.Key });
                    if (AuthorUpinsert)
                        if (KeystoRemove.TryGetValue(author.Key, out int value))
                        {
                            _context.Authors.Update(new() { AuthorId = value, Bio = author.Value ,FullName = author.Key });
                            continue;
                        }
                    _context.Authors.Add(new() { Bio = author.Value, FullName = author.Key });

                }

                //_context.Authors.AddRange(AuthortoInsert);
            }
        }


        if (data.Category is not null)
        {
            var filterdatadist = data.Category
                .DistinctBy(a => a.Name.ToLower())
                .Select(x => x.Name).ToList();


            var CategoriestoRemove = _context.Categories.Where(a => filterdatadist.Contains(a.Name)).Select(p => p.Name).ToList();


            foreach (var x in CategoriestoRemove)
            {
                filterdatadist.Remove(x);
            }

            if (filterdatadist.Count > 0)
            {

                var categoriesToInsertList = new List<Category>();


                foreach (var cat in filterdatadist)
                {
                    categoriesToInsertList.Add(new Category { Name = cat });
                }

                _context.Categories.AddRange(categoriesToInsertList);
            }
        }




        try
        {
            await _context.SaveChangesAsync();
            return true;
        }
       catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return false;
        }



    }















}
