using Database.Services;
using Microsoft.AspNetCore.Mvc;
using Database.ApplicationDbcontext;

using Database.Model.Apimodels;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Auth._3rdpartyPaymentportal;
using Database.Model;
using Database.Model.ModelsDto;
using Database.Model.ModelsDto.PaymentPartialmodels;
using Database.Mapperdtotodb;
using System.Diagnostics.Eventing.Reader;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Data.SqlTypes;
using System.ComponentModel;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Bookstore_backend.Controllers
{
    [Route("api")]
    [ApiController]
    public class BookStoreController : ControllerBase
    {
        private readonly ICrudlayer dbcall;


        public BookStoreController(ICrudlayer crud)
        {
            dbcall = crud;

        }


        // GET: api/<testapi>

        [HttpGet]
        [Route("booklist")]
        
        public async Task<ActionResult<BooksCatalog>> GetList([FromQuery] Pagemodel pagesettings, CancellationToken cToken)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var data = await dbcall.RawReturn(pagesettings.Page, pagesettings.Pagesize, cToken);

            if (data.Count == 0)
            {
                return NotFound();
            }
            else return Ok(data);

            //var x = new BooksContextFactory();
        }





        /// <summary>
        /// Retrieves a list of books based on the specified filter criteria.
        /// </summary>   
        [HttpGet]
        [Route("OnMatch")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(DetailedFilterBookModel))]
        public async Task<IActionResult> GetListfiltered([FromQuery] QuerySelector selector, CancellationToken cToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var data = await dbcall.Filteredquery(selector, cToken);

            if (data.Count == 0)
            {
                return NotFound();
            }
            else
            {
                return Ok(data);
            }

            //var x = new BooksContextFactory();
        }







        //get {isbn,price,stockqnty} service ,
        //tier2 accounts have no restrictions
        //This is a subscription-based service  
        //todo : free books,
        //todo : coupon code like(30% off all fantasy books /specific author name)
        /// <summary>
        ///
        /// </summary>   
        /// <param name="apikey">The API key used to authenticate the request.</param>   
        /// <remarks>
        /// Sample request:
        /// 
        ///     GET /
        ///     
        ///    curl -X 'GET' /  
        ///    'https://localhost:7178/api/0000000000001'
        ///    -H 'accept: application/json' 
        ///    -H 'x-api-key: 00000000-0000-0000-0000-4ae86ca65fc2'
        ///    
        /// </remarks>
        [HttpGet("{ISBN}")]
        //[Authorize]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MarketDataAPIModelbyISBN))]
        public async Task<IActionResult> ApiService([FromRoute][RegularExpression("^[0-9]*$")] string ISBN, [FromHeader(Name = "x-api-key")] string apikey , CancellationToken cToken)
        {


            //(Guid UserdbGuid, UserRole role) = (Guid.Empty,UserRole.user);

            //foreach (var claims in User.Claims)
            //{

            //    switch (claims.Type)
            //    {
            //        case "UserID":
            //            {
            //                if (claims.Value.IsNullOrEmpty()) return BadRequest();
            //                else UserdbGuid = Guid.Parse(claims.Value);
            //            }; break;
            //        case "ruoli":
            //            {
            //                if (claims.Value.IsNullOrEmpty()) return BadRequest();

            //                if (claims.Value == "user") role = UserRole.user;
            //                else if (claims.Value == "admin") role = UserRole.admin;
            //                else return BadRequest();


            //            }; break;

            //    }

            //}


            //var data = await dbcall.ApiServiceGetbyisbn(ISBN,UserdbGuid,role, cToken);

            //if (data is null)
            //{
            //    return NotFound(data);
            //}
            //return Ok(data);
           var(data,Errors)  =  await dbcall.ApiServiceGetbyisbn(ISBN , Guid.Parse(apikey) , cToken);


            return data is not null ? Ok(data) :  StatusCode(Errors!.Code, Errors);


            //if(data is not null) return Ok(data);

            //switch (Errors!.Code)
            //{
            //    case 400: return BadRequest(Errors);
            //    case 403: return Forbid();
            //    case 404: return NotFound(Errors);
            //    case 409: return Conflict(Errors);
            //    case 0: return StatusCode(Errors.Code,Errors);   
            //    default:
            //        break;
            //}

           

            //return StatusCode(500);

        }

       





        
        [HttpPost]
        [Authorize]
        [Route("buybook")]
        public async Task<IActionResult> UserBuyTransaction([FromBody] BookPartialPaymentModel data ,[FromServices] PaymentPortalx portalpay) 
        {

            var UserID = User.Claims.SingleOrDefault(x => x.Type == "UserID");


            if (UserID is null) return BadRequest();
            if (!Guid.TryParse(UserID.Value, out Guid GuidUserID))return BadRequest();

         

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            

            var dbdata = await dbcall.GetInvoicebooks(data.BookItemList, GuidUserID);
            if(dbdata.Item1 is null || dbdata.Item2 is null) return BadRequest();



            dbdata.Item1.Fillcardinfo(data.PaymentDetails);


            var message = await portalpay.BookPaymentportal(dbdata.Item1, (int)dbdata.Item2, dbcall);


            


            switch (message.Code)
            {
                case 200: return Ok(dbdata.Item1.Invoce);   
                case 400: return StatusCode(400, message.Message);

                default: return StatusCode(500);
            };


            
            
        }







        //tier 0 : 5 calls per 5 min 
        //tier 1 : 10 calls per 5 min (monthly bill)
        //tier 2 no limit (monthly bill || admin)
        //not implemented //tier 3 : $ per call (monthly bill || per call bill)

        /// <param name="subscriptionTier">
        /// - <c>Tier0</c>: Represents the basic subscription tier.
        /// - <c>Tier1</c>: Represents the standard subscription tier.
        /// - <c>Tier2</c>: Represents the premium subscription tier.
        /// </param>
        [HttpPost]
        //[Authorize]
        [Route("buysubTier")] // default = 0 XX
        [Authorize]
        public async Task<IActionResult> BuySubscriptions([FromBody] PartialPaymentDetails data,[FromQuery] Subscription subscriptionTier, [FromServices] PaymentPortalx portalpay) 
        {



            Guid UserdbGuid = Guid.Empty;

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

                            if (claims.Value == "user") break;
                            else if (claims.Value == "admin") return BadRequest("admin account doesn't need a subscription");
                            else return BadRequest();


                        };

                }

            }

            //var backdata = await portalpay.Subpayment(Guid.Parse("4a60ac31-5117-4bc5-ad7b-e09f861e6651"), subscriptionTier, dbcall);
            var backdata = await portalpay.Subpayment(UserdbGuid, subscriptionTier, dbcall);


            return backdata.Code == 200 ? Ok():StatusCode(500) ;
        }














            //[HttpGet]
            //[Route("testConcurr/{delay}/{qnty}")]

            //public async Task<IActionResult> Testapi([FromRoute]int delay, [FromRoute] int qnty)
            //{

            //  await  dbcall.ConcurTest(delay,qnty);    

            //    return Ok();
            //}



            //[HttpGet]
            //[Route("test")]

            //public async Task<IActionResult> Testapi()
            //{
            //    await dbcall.Testapi();




            //    return Ok();
            //}













        }


}
