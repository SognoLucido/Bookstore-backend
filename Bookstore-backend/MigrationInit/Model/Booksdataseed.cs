using Database.Model;

namespace Bookstore_backend.MigrationInit.Model
{
    public class Booksdataseed
    {
        public List<Book> books { get; set; }
        public List<Author> authors { get; set; }
        public List<Category> categories { get; set; }

    }
}
