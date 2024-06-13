


using System.ComponentModel.DataAnnotations;

namespace Database.Model
{
    
        public class Order
        {
            [Key]
            public int OrderId { get; set; }

            public int CustomerId { get; set; }
            public Customer Customer { get; set; }
            public DateTime OrderDate { get; set; }
            public string Status { get; set; }
            public ICollection<OrderItem> OrderItems { get; set; }
        }

        public class OrderItem
        {
            [Key]
            public int OrderItemId { get; set; }
            public int OrderId { get; set; }
            public Order Order { get; set; }
            public int BookId { get; set; }
            public Book Book { get; set; }
            public int Quantity { get; set; }
            public decimal Price { get; set; }
        }


        public class Customer
        {
            [Key]
            public int CustomerId { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Email { get; set; }
            public string Password { get; set; }
            public string Salt {  get; set; }            
            public string Address { get; set; }
            public string Phone { get; set; }
            public ICollection<Order> Orders { get; set; }
        }

    
}
