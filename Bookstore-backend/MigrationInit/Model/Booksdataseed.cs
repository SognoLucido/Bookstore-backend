using Database.Model;

namespace Bookstore_backend.MigrationInit.Model
{
    public class Booksdataseed
    {
        public IEnumerable<Book> books { get; set; }
        public IEnumerable<Author> authors { get; set; }
        public IEnumerable<Category> categories { get; set; }

    }
}
