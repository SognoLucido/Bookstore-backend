﻿
using System.ComponentModel.DataAnnotations;


namespace Database.Model.ModelsDto.Paymentmodels
{
    public class BookPaymentModel
    {

        //public string Token { get; set; } idk

        [Required]
        public PaymentDetails PaymentDetails { get; set; }


        [Required]
        public List<BookItemList> bookItemList { get; set; }

    }


    public class BookItemList
    {
        [MaxLength(13)]
        [RegularExpression("^[0-9]*$")]
        public string ISBN { get; set; }


        public int Quantity { get; set; }

    }


    public class PaymentDetails
    {
        public string CardHolderName { get; set; }
        public string CardNumber { get; set; }
        public CardExpiry CardExpiry { get; set; }
        public string CardCVC { get; set; }

        public List<Invoice> Invoce { get; set; } = [];

        public Decimal TotalAmount { get; set; }


    }


    public class CardExpiry
    {
        public int Year { get; set; }
        public int Month { get; set; }

    }

 

    public record Invoice(string Title, string ISBN, decimal PricePerItem ,int? Quantity);



}
