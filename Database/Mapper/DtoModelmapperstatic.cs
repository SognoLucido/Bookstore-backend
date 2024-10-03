using Database.Model;
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

        public static List<Book> MapTobookList(this IEnumerable<BookinsertModel> data)
        {
            List<Book> books = [];


            foreach (var model in data)
            {
                books.Add(new Book
                {
                    Title = model.Title,


                    Author = new()
                    {
                        // AuthorId
                        FullName = model.Author
                    },
                    Category = new()
                    {
                        //CategoryId
                        Name = model.Category
                    },
                    ISBN = model.ISBN,
                    Price = model.Price,
                    StockQuantity = model.StockQuantity,
                    PublicationDate = new DateOnly(model.PublicationDate.year, model.PublicationDate.month, model.PublicationDate.day),
                    Description = model.Description,

                });
            }

            return books;

        }

        public static Book MapTobook(this BookinsertModel model)
        {

            var book = new Book
            {
                Title = model.Title,     
                ISBN = model.ISBN,
                Price = model.Price,
                StockQuantity = model.StockQuantity,
                PublicationDate = new DateOnly(model.PublicationDate.year, model.PublicationDate.month, model.PublicationDate.day),
                Description = model.Description,

            };


            return book;

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


        public static DetailedFilterBookModel DbbookmodelToApireturnBookmodel(this Book data)
        {

            return new DetailedFilterBookModel
            {
                BookTitle = data.Title,
                Category = data.Category.Name,
                Price = data.Price.ToString() + "$",
                Description = data.Description,
                AuthorName = data.Author.FullName,
                ISBN = data.ISBN,
                PublicationDate  = data.PublicationDate,
            };
        }




    }
}
