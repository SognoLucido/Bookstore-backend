

using Database.Model;
using Database.Model.ModelsDto;
using Database.Model.ModelsDto.Paymentmodels;
using Database.Services;





namespace Auth._3rdpartyPaymentportal
{
    public class PaymentPortalx
    {
        private readonly HttpClient _httpClient;
        //private readonly ICrudlayer dbcall;


        public PaymentPortalx(HttpClient httpClient)
        {
            _httpClient = httpClient;
            //dbcall = _dbcall;
          
        }

        public async Task<Respostebookapi> BookPaymentportal(PaymentDetails payment,int orderid, ICrudlayer dbcall)
        {

            /*
            var response = await _httpClient.PostAsJsonAsync("https://api.example.com/payment", payment);

            try
            {
                response.EnsureSuccessStatusCode();

                return new Respostebookapi
                {
                    Code = 200                  
                };


            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine(ex.Message);

                return new Respostebookapi 
                {
                    Code = 400,
                    Message = ex.Message,
                };
            }
            */



            //if try is successful ,before the return :

           await dbcall.UpdateOrderStatus(orderid, Status.Accepted);

            //
            //in the catch before the return :
            // await dbcall.UpdateOrderStatus(orderid,Status.Declined);
           


            //


            // call others services  


            return new Respostebookapi { Code = 200 };



        }




        public async Task<Respostebookapi> Subpayment(Guid UserID,  Subscription subtier,ICrudlayer dbcall)
        {

            var paymentbody = new { UserID, subtier };


            //await dbcall.Testapi();


            //var response = await _httpClient.PostAsJsonAsync("https://api.example.com/paymentservicemicroservice??paymentportal", paymentbody);

            //try
            //{
            //    response.EnsureSuccessStatusCode();



            if (await dbcall.UpdateTier(UserID, subtier, _httpClient))
            {
                return new Respostebookapi()
                {
                    Code = 200,
                };

            }
            else
            {
                return new Respostebookapi()
                {
                    Code = 500,
                };
            }

            //}
            //catch (HttpRequestException requestex)
            //{
            //    Console.WriteLine(requestex.Message);

            //    return new Respostebookapi
            //    {
            //        Code = 500,
            //        Message = requestex.Message +"backend to paymentservice badrequest ",
            //    };

            //}
            //catch (Execption ex)
            //{
            //    Console.WriteLine(ex.Message);

            //    return new Respostebookapi
            //    {
            //        Code = 500,
            //        Message = ex.Message +"others stuff",
            //    };
            //{

            //}














        }


    }
}
