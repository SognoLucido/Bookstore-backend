
using Dblogic.Applicationcontext;
using Dblogic.Services;
using Microsoft.Extensions.Configuration;


namespace Dblogic
{
    public class DbBookCrud : ICrudlayer
    {
        private readonly IConfiguration _config;
        private readonly AppdbContext _context;
        public DbBookCrud(IConfiguration config , AppdbContext context)          
        {
            _config = config;
            _context = context;
        }





    }
}
