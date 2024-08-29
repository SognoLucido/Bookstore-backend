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
using Database.Model.ModelsDto.Paymentmodels;
using Microsoft.EntityFrameworkCore.Storage.Json;


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

        /// <summary>
        /// substring matching with limit result hardcoded to 5 
        /// </summary>
        /// <param name="booktitle">optional</param>
        /// <param name="authorname">optional</param>
        /// <param name="category">optional</param>   
        /// <returns></returns>
        [HttpGet]
        [Route("search")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(DetailedFilterBookModel))]
        public async Task<IActionResult> SearchItems([FromQuery] string? booktitle, [FromQuery] string? authorname, [FromQuery] string? category, CancellationToken cToken)
        {

            const int limit = 5;

            var data = (booktitle, authorname, category);



            var test = await dbcall.SearchItems(limit,data,cToken);

            return Ok(test);

         }

        [HttpGet]
        [Route("booklist")]
        
        public async Task<ActionResult<BooksCatalog>> GetBookList([FromQuery] Pagemodel pagesettings, CancellationToken cToken)
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





        ///// <summary>
        ///// Retrieves a list of books based on the specified filter criteria.
        ///// </summary>   
        //[HttpGet]
        //[Route("exactmatch")]
        //[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(DetailedFilterBookModel))]
        //public async Task<IActionResult> GetListfiltered([FromQuery] QuerySelector selector, CancellationToken cToken)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    var data = await dbcall.Filteredquery(selector, cToken);

        //    if (data.Count == 0)
        //    {
        //        return NotFound();
        //    }
        //    else
        //    {
        //        return Ok(data);
        //    }

        //    //var x = new BooksContextFactory();
        //}







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
        ///    -H 'x-api-key: 000000000000000000004ae86ca65fc2'
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

       











        }


}
