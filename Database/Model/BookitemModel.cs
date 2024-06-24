
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;



namespace Database.Model
{
   
        public class Author
        {
            [Key]
            public int AuthorId { get; set; }

            [MaxLength(30)]
            public string FullName { get; set; }
            public string Bio { get; set; }
            public ICollection<Book> Books { get; set; }
        }

        public class Category
        {
            [Key]
            public int CategoryId { get; set; }

            [MaxLength(30)]
            public string Name { get; set; }
        }

        public class Book
        {
            [Key]
            public int BookId { get; set; }
            public string Title { get; set; }
            public int AuthorId { get; set; }
            public Author Author { get; set; }
            public int CategoryId { get; set; }
            public Category Category { get; set; }
            public string ISBN { get; set; }

        //[Timestamp]
        //public string RowVersion { get; set; }

        [Column(TypeName = "decimal(8,2)")]
           public decimal Price { get; set; }
            public int StockQuantity { get; set; }
            public DateOnly PublicationDate { get; set; }
            public string Description { get; set; }
        }

       

     
    
}
