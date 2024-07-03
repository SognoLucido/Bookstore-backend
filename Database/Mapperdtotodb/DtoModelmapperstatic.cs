using Database.Model;
using Database.Model.Apimodels;
using Database.Model.ModelsDto;


namespace Database.Mapperdtotodb
{
    internal static class DtoModelmapperstatic
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
                RolesModelId = 2


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







    }
}
