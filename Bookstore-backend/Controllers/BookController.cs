using Database.Services;
using Microsoft.AspNetCore.Mvc;
using Database.ApplicationDbcontext;

using Database.Model.Apimodels;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Database.Model.ModelsDto.Paymentmodels;
using System.Collections.Generic;
using Auth._3rdpartyPaymentportal;
using Database.Model;
using Database.Model.ModelsDto;
using Database.Model.ModelsDto.PaymentPartialmodels;



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
        [Route("Catalog")]
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






        [HttpGet]
        [Route("OnMatch")]
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








        [HttpGet("{ISBN}")]
        [Authorize("Userlogged")]
        public async Task<IActionResult> GetbyISBN([FromRoute][RegularExpression("^[0-9]*$")] string ISBN, CancellationToken cToken)
        {


            var data = await dbcall.Getbyisbn(ISBN, cToken);

            if (data is null)
            {
                return NotFound(data);
            }
            return Ok(data);


        }

        [HttpDelete("{ISBN}")]
        [Authorize("AdminOnly")]
        public async Task<IActionResult> DeletebyISBN([FromRoute][RegularExpression("^[0-9]*$")] string ISBN, CancellationToken cToken)
        {

            if (await dbcall.Deletebyisbn(ISBN, cToken))
            {
                return Ok();
            }
            return NotFound();


        }




        [HttpPost]
        //[Authorize]
        [Route("buy")]
        public async Task<IActionResult> UserBuyTransaction([FromBody] BookPartialPaymentModel data ,[FromServices] PaymentPortalx portalpay) 
        { 
        
          //var x = User.Claims.SingleOrDefault();





          //  if (x is not null && x.Subject is not null && x.Subject.IsAuthenticated)
          //  {




                //method

                var zuz = await dbcall.GetInvoicebooks(data.BookItemList);





                //if (await portalpay.Paymentportal(data.PaymentDetails))
                //{




                //    return Ok(x);
                //}
                //else
                //{
                //    return BadRequest("payment failed");
                //}


            //    return Ok();
            //}

            if(zuz is null)
            {
                BadRequest();
            }
            else return Ok(zuz); 

            return Ok();
            
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







        [HttpPatch("{ISBN}")]
        //[Authorize("AdminOnly")]
        public async Task<IActionResult> AddOrOverrideStockQuantitybyISBN(
            [FromRoute][MaxLength(30)][RegularExpression("^[0-9]*$")] string ISBN,
            [FromQuery][Required] int qnty,
            [FromQuery] bool? ForceOverride, // if true override the current dbstock-qnty with the qnty  , if not just add += qnty to the dbstock qnty 
            CancellationToken cToken)
        {

            //


            if (await dbcall.AddOrOverrideStockQuantitybyISBN(ISBN, qnty, ForceOverride ?? false, cToken))
            {
                return Ok();
            }
            return NotFound();


        }


        [HttpPost]
        //[Authorize("AdminOnly")]
        [Route("Book")]
        public async Task<IActionResult> InsertBook([FromBody] BookinsertModel bodydata )
        {

            if (!ModelState.IsValid ) return BadRequest(bodydata);

            if (bodydata.PublicationDate.year > DateTime.UtcNow.Year) return BadRequest("wrong year");

            //try
            //{
            //    _ = new DateOnly(bodydata.PublicationDate.year, bodydata.PublicationDate.month, bodydata.PublicationDate.day);
            //}
            //catch (Exception)
            //{
            //    return BadRequest(" invalid date ");
            //}

            var message = await dbcall.InsertBookItem(bodydata);


            switch (message.Code)
            {
                case 200: return Ok(message.Message); 
                case 500: return StatusCode(500, message.Message); 
                case 404: return NotFound(message.Message);
                case 409: return StatusCode(409, message.Message); 

                default: return BadRequest();
            }


            
        }





    }


}
