using Database.ApplicationDbcontext;
using Database.Model;
using Database.Services;


namespace Database.DatabaseLogic;

public class DbBookCrud : ICrudlayer
{

    private readonly Booksdbcontext _context;
    public DbBookCrud(Booksdbcontext context)
    {

        _context = context;
    }


    public async Task<Person> Getbyid(int id)
    {

        var x = await _context.Person.FindAsync(id);

        return x;


    }

    public async Task Insert(Person person)
    {

        _context.Add(person);
        var x = await _context.SaveChangesAsync();



    }



}
