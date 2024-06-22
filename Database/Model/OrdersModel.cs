


using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Model
{



   public enum Status
    {
      Declined,
      Accepted
    }


    public class Order
    {
        [Key]
        public int OrderId { get; set; }

        public Guid CustomerId { get; set; }
        public Customer Customer { get; set; }
        public DateTime OrderDate { get; set; }

        public Status status { get; set; }
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

        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }
    }


    public class Customer
    {
        [Key]
        public Guid Id { get; set; }

        [MaxLength(30)]
        public string FirstName { get; set; }


        [MaxLength(30)]
        public string LastName { get; set; }

        [MaxLength(40)]
        public string Email { get; set; }

        [MaxLength(64)]
        public string Password { get; set; }

        [MaxLength(8)]
        public string Salt { get; set; }

        [MaxLength(40)]
        public string Address { get; set; }

        [MaxLength(20)]
        public string Phone { get; set; }
        public List<Order> Orders { get; set; }

        //public List<RolesModel> RolesModel { get; set; }

        public int RolesModelId {  get; set; }
        public RolesModel RolesModel { get; set; }

    }







}
