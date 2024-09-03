using Database.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Database.Model.ModelsDto.Paymentmodels;
using Database.Model.ModelsDto;
using Auth._3rdpartyPaymentportal;
using Database.Model.ModelsDto.PaymentPartialmodels;
using Database.Model;
using Database.Mapperdtotodb;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Bookstore_backend.Controllers
{
 
    [Route("api/")]
    [ApiController]
    public class UserController : ControllerBase
    {

        private readonly ICrudlayer dbcall;
        public UserController(ICrudlayer crud)
        {
            dbcall = crud;

        }
        /// <summary>
        /// Get account information like -> apikey
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("userinfo")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserInfo))]
        public async Task<IActionResult> AccountInfoGet([FromServices] TokenBlocklist block)
        {
            var UserID = User.Claims.SingleOrDefault(x => x.Type == "UserID");


            if (!Guid.TryParse(UserID.Value, out Guid userIDokcheck)) return BadRequest();


            //if (User.HasClaim("ruoli", "admin"))return BadRequest("Only Users");

            var dbdata = await dbcall.GetUserInfoAccount(userIDokcheck);



            //var rawtoken = Request.Headers["Authorization"].First();


            //string EncodedSignature = rawtoken.Substring(rawtoken.Length - 43);

            //block.TokenInsert(EncodedSignature);

            //await Console.Out.WriteLineAsync($"insert {EncodedSignature}");


            return dbdata is not null ? Ok(dbdata) : NotFound("invalid UserID - user not found");



            //return Ok(await dbcall.GetUserInfoAccount(Guid.Parse("815edd39-8264-4dba-a76d-833aff20fd7f")));
        }



        [HttpPost]
        [Authorize]
        [Route("buybook")]
        public async Task<IActionResult> UserBuyTransaction([FromBody] BookPartialPaymentModel data, [FromServices] PaymentPortalx portalpay)
        {

            var UserID = User.Claims.SingleOrDefault(x => x.Type == "UserID");


            if (UserID is null) return BadRequest();
            if (!Guid.TryParse(UserID.Value, out Guid GuidUserID)) return BadRequest();
            if(User.HasClaim("role","admin")) return BadRequest("only users");


            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }



            var dbdata = await dbcall.GetInvoicebooks(data.BookItemList, GuidUserID);
            if (dbdata.Item1 is null || dbdata.Item2 is null) return BadRequest();



            dbdata.Item1.Fillcardinfo(data.PaymentDetails);


            var message = await portalpay.BookPaymentportal(dbdata.Item1, (int)dbdata.Item2, dbcall);





            switch (message.Code)
            {
                //case 200: return Ok((dbdata.Item1.Invoce,dbdata.Item1.TotalAmount))
                case 200: return Ok(new Invoicev2(dbdata.Item1.Invoce, dbdata.Item1.TotalAmount));
                case 400: return StatusCode(400, message.Message);

                default: return StatusCode(500);
            };




        }

        //rip
        private record Invoicev2(List<Invoice> Invoice, decimal Total);




        //tier 0 : 5 calls per 5 min 
        //tier 1 : 10 calls per 5 min (monthly bill)
        //tier 2 no limit (monthly bill || admin)
        //not implemented //tier 3 : $ per call (monthly bill || per call bill)

        


        /// <param name="subscriptionTier">
        /// - <c>Tier0</c>: Represents the free tier(default).
        /// - <c>Tier1</c>: Represents the standard subscription tier.
        /// - <c>Tier2</c>: Represents the premium subscription tier.
        /// </param>
        [HttpPost]
        [Route("buysubtier")] // default = 0 XX
        [Authorize]
        public async Task<IActionResult> BuySubscriptions([FromBody] PartialPaymentDetails data, [FromQuery] Subscription subscriptionTier, [FromServices] PaymentPortalx portalpay)
        {



            Guid UserdbGuid = Guid.Empty;
            UserRole? role = null;

            foreach (var claims in User.Claims)
            {

                switch (claims.Type)
                {
                    case "UserID":
                        {
                            if (claims.Value.IsNullOrEmpty()) return BadRequest();
                            else UserdbGuid = Guid.Parse(claims.Value);
                        }; break;
                    case "ruoli":
                        {
                            if (claims.Value.IsNullOrEmpty()) return BadRequest();

                            if (claims.Value == "user") break ; 
                            else if (claims.Value == "admin") return BadRequest("admin account doesn't need to buy a subscription");
                            else return BadRequest();
                         

                        };

                }

            }

            //var backdata = await portalpay.Subpayment(Guid.Parse("4a60ac31-5117-4bc5-ad7b-e09f861e6651"), subscriptionTier, dbcall);

       


            var  backdata = await portalpay.Subpayment(UserdbGuid, subscriptionTier, dbcall);
          



            return backdata.Code == 200 ? Ok() : StatusCode(500);
        }




        /// <summary>
        /// Only the account with the "user" role as flag can delete itself. To delete an admin account, you must first downgrade the role from admin to user
        /// </summary>
        /// <returns></returns>
        [HttpDelete]
        [Authorize("UserOnly")]
        [Route("accountself")]
        public async Task<IActionResult> DeleteAccountSelf([FromServices] TokenBlocklist block)
        {
            var UserIDdata = User.FindFirst("UserID").Value;
            if (!Guid.TryParse(UserIDdata, out Guid GuidUserID)) return BadRequest();



            if (await dbcall.DeleteAccount(GuidUserID))
            {
                var rawtoken = Request.Headers.Authorization.First();

                string EncodedSignature = rawtoken.Substring(rawtoken.Length - 43);

                block.TokenInsert(EncodedSignature);

                return Ok("Deletion was successful");
            }
            else return StatusCode(500, "Unsuccessful");


        }





        //OLD

        //[HttpDelete]
        //[Authorize]
        //[Route("account")]
        //public async Task<IActionResult> DeleteAccount(
        //    [FromQuery] Guid? userid, 
        //    [FromQuery][EmailAddress] string? email,
        //    [FromServices] TokenBlocklist block, CancellationToken ctoken)
        //{
            

        //    var Usercheck = User.HasClaim("ruoli", "user");


        //    var Admincheck = User.HasClaim("ruoli", "admin");

        //    var UserIDdata = User.FindFirst("UserID").Value;
        //    if(!Guid.TryParse(UserIDdata, out Guid GuidUserID))return BadRequest();


        //    if (Usercheck && (userid is not null || email is not null)) return Unauthorized("only admins can delete other users , leave it blank");
        //    if(Admincheck && GuidUserID == userid) return BadRequest();



        //    var Role = Usercheck ? UserRole.user : UserRole.admin;
            

        //    //foreach (var claims in User.Claims)
        //    //{

        //    //    switch (claims.Type)
        //    //    {
        //    //        case "UserID":
        //    //            {
        //    //                if (claims.Value.IsNullOrEmpty()) return BadRequest();
        //    //                else UserdbGuid = Guid.Parse(claims.Value);
        //    //            }; break;
        //    //        case "ruoli":
        //    //            {
        //    //                if (claims.Value.IsNullOrEmpty()) return BadRequest();
        //    //                else Role = claims.Value;
        //    //            }; break;

        //    //    }

        //    //}




        //    if(Role == UserRole.user)
        //    {
        //        var rawtoken = Request.Headers.Authorization.First();

        //        string EncodedSignature = rawtoken.Substring(rawtoken.Length - 43);

        //        block.TokenInsert(EncodedSignature);
        //    }

            



        //    switch (Role)
        //    {
        //        case UserRole.user:
        //            {
        //                if (await dbcall.DeleteAccount(GuidUserID)) return Ok("Deletion was successful");

        //            }; break;

        //        case UserRole.admin:
        //            {
        //                if (email is not null)
        //                {
        //                    if (await dbcall.DeleteAccount(email)) return Ok("Deletion was successful");
        //                }
        //                else
        //                {
        //                    if (await dbcall.DeleteAccount(GuidUserID)) return Ok("Deletion was successful");

        //                }

        //            }; break;

        //    };





        //    return StatusCode(500, "Deletion failed");
        //}



        //// POST api/<UserController>
        //[HttpPost]
        //public void Post([FromBody] string value)
        //{
        //}

        //// PUT api/<UserController>/5
        //[HttpPut("{id}")]
        //public void Put(int id, [FromBody] string value)
        //{
        //}

        //// DELETE api/<UserController>/5
        //[HttpDelete("{id}")]
        //public void Delete(int id)
        //{
        //}
    }
}
