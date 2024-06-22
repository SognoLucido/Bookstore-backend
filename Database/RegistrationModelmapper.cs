using Database.Model;
using Database.Model.Apimodels;


namespace Database
{
   internal static class RegistrationModelmapper
    {

        public static Customer Maptodbregistration(this Registration model)
        {

            return  new Customer
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
    }
}
