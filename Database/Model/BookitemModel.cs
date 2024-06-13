using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;


namespace Database.Model
{
   
        public class Author
        {
            [Key]
            public int AuthorId { get; set; }

            public string Name { get; set; }
            public string Bio { get; set; }
            public ICollection<Book> Books { get; set; }
        }

        public class Category
        {
            [Key]
            public int CategoryId { get; set; }
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
            public decimal Price { get; set; }
            public int StockQuantity { get; set; }
            public DateTime PublicationDate { get; set; }
            public string Description { get; set; }
        }

       

     
    
}
