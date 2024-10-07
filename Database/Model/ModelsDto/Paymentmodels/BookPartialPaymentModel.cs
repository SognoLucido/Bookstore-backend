
using System.ComponentModel.DataAnnotations;


namespace Database.Model.ModelsDto.PaymentPartialmodels
{
    public class BookPartialPaymentModel
    {

        //public string Token { get; set; } idk

        [Required]
        public PartialPaymentDetails PaymentDetails { get; set; }


        [Required]
        public List<BookItemList> BookItemList { get; set; }

    }


    public class BookItemList
    {
        [MaxLength(13)]
        [RegularExpression("^[0-9]*$")]
        public string ISBN { get; set; }

        [Required]
        [Range(0, 500)]
        public int Quantity { get; set; }

    }


    public class PartialPaymentDetails
    {
        [Required]
        public string CardHolderName { get; set; }

        [Required]
        [RegularExpression("^[0-9]{16}$")]
        public string CardNumber { get; set; }

        [Required]
        public CardExpiry CardExpiry { get; set; }


        [MaxLength(3)]
        [RegularExpression("^[0-9]+$")]
        public string CardCVC { get; set; }

        //public List<Invoice> Invoce { get; set; }

        //public Decimal TotalAmount { get; set; }


    }


    public class CardExpiry
    {
        [Range(24,99)]
        public int Year { get; set; }

        [Range(1,12)]
        public int Month { get; set; }

    }

 

    //public record Invoice(string Title, string ISBN, decimal Price);



}
