using Database.Services;
using Microsoft.AspNetCore.Mvc;
using Database.Model.Apimodels;
using System.ComponentModel.DataAnnotations;



// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Bookstore_backend.Controllers
{

    [ApiController]
    public class BookStoreController : ControllerBase
    {
        private readonly ICrudlayer dbcall;


        public BookStoreController(ICrudlayer crud)
        {
            dbcall = crud;

        }

        /// <summary>
        /// substring matching with limit result hardcoded to 5 
        /// </summary>
        /// <param name="booktitle">optional</param>
        /// <param name="authorname">optional</param>
        /// <param name="category">optional</param>   
        /// <returns></returns>
        [HttpGet]
        [Route("api/search")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(DetailedFilterBookModel))]
        public async Task<IActionResult> SearchItems([FromQuery] string? booktitle, [FromQuery] string? authorname, [FromQuery] string? category, CancellationToken cToken)
        {

            const int limit = 5;

            var dataToinsert = (booktitle, authorname, category);



            var databack = await dbcall.SearchItems(limit,dataToinsert,cToken);



            return databack.Count > 0 ?  Ok(databack) : NotFound();

         }

        [HttpGet]
        [Route("api/booklist")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<BooksCatalog>))]
        public async Task<IActionResult> GetBookList([FromQuery] Pagemodel pagesettings, CancellationToken cToken)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var data = await dbcall.RawReturn(pagesettings.Page, pagesettings.Pagesize, cToken);

           

           return data.Count() > 0 ? Ok(data) : NotFound();
        }





        //get {isbn,price,stockqnty} service ,
        //tier2 accounts have no restrictions
        //This is a subscription-based service  
        //todo : free books,
        //todo : coupon code like(30% off all fantasy books /specific author name)
        /// <summary>
        ///Simulates a premium service. It requires the x-api-key header for authentication. Check the /api/userinfo endpoint (login required) for the API key
        /// </summary>   
        /// <param name="apikey">The API key used to authenticate the request.</param>   
        /// <remarks>Is
        /// Sample request:
        /// 
        ///     GET /
        ///     
        ///    curl -X 'GET' /  
        ///    'https://localhost:7178/apikey/0000000000001'
        ///    -H 'accept: application/json' 
        ///    -H 'x-api-key: 000000000000000000004ae86ca65fc2'
        ///    
        /// </remarks>
        [HttpGet("apikey/{ISBN}")]
        //[Authorize]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MarketDataAPIModelbyISBN))]
        public async Task<IActionResult> ApiService([FromRoute][RegularExpression("^[0-9]{13}$")] string ISBN, [FromHeader(Name = "x-api-key")] string apikey , CancellationToken cToken)
        {


          
            if(!Guid.TryParse(apikey, out var key))return BadRequest("invalid key");

           
           var(data,Errors)  =  await dbcall.ApiServiceGetbyisbn(ISBN , key , cToken);


            return data is not null ? Ok(data) :  StatusCode(Errors!.Code, Errors);


         

        }













    }


}
