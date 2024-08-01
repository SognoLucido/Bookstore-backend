﻿using Database.Model;
using Database.Model.Apimodels;
using Database.Model.ModelsDto;
using Database.Model.ModelsDto.Paymentmodels;
using Database.Model.ModelsDto.PaymentPartialmodels;



namespace Database.Mapperdtotodb
{
    public static class DtoModelmapperstatic
    {

        public static Customer MaptoCustomer(this Registration model)
        {

            return new Customer
            {
                Id = Guid.NewGuid(),
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                //pass
                //salt
                Address = model.Address,
                Phone = model.Phone,
                RolesModelId = 2,
               
                

            };
        }

        public static Book MapTobook(this BookinsertModel model)
        {
            return new Book
            {
                 Title = model.Title,
                // AuthorId
                //CategoryId
                ISBN = model.ISBN,
                Price = model.Price,
                StockQuantity = model.StockQuantity,
                PublicationDate = new DateOnly(model.PublicationDate.year, model.PublicationDate.month, model.PublicationDate.day),
                Description = model.Description,

            };

        }

        public static PaymentDetails Fillcardinfo(this PaymentDetails model, PartialPaymentDetails partial)
        {

            model.CardHolderName = partial.CardHolderName;
            model.CardNumber = partial.CardNumber;
            model.CardExpiry = new()
            {
                Month = partial.CardExpiry.Month,
                Year = partial.CardExpiry.Year,
            };
            model.CardCVC = partial.CardCVC;


            return model;
        }







    }
}
