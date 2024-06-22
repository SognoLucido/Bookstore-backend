using Database.ApplicationDbcontext;
using Database.Model;
using Database.Model.Apimodels;
using Database.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using static System.Runtime.InteropServices.JavaScript.JSType;


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

   

    public async Task<bool> Registration(Registration regi,CancellationToken token = default)
    {

       var Findemailexist = await _context.Customers.Where(a => a.Email == regi.Email.ToLower()).FirstOrDefaultAsync(token);

        if(Findemailexist is not null ) return false;

        // procedo con la registrazione


        //creo il modello base da inserire nel database
        var modeltoinsert = regi.Maptodbregistration();

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
                throw;
            }

        
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
