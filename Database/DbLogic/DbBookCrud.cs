using Database.ApplicationDbcontext;
using Database.Model;
using Database.Model.Apimodels;
using Database.Services;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Linq;


namespace Database.DatabaseLogic;

public class DbBookCrud : ICrudlayer
{

    private readonly Booksdbcontext _context;
    public DbBookCrud(Booksdbcontext context)
    {
        _context = context;
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

   


}
