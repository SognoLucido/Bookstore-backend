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
using Database.Mapperdtotodb;
using System.Diagnostics.Eventing.Reader;


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







        //apikeytoadd
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




        [HttpPost]
        [Authorize]
        [Route("buy")]
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


            var message = await portalpay.Paymentportal(dbdata.Item1, (int)dbdata.Item2, dbcall);


            switch (message.Code)
            {
                case 200: return Ok(dbdata.Item1.Invoce);   
                case 400: return StatusCode(400, message.Message);

                default: return StatusCode(500);
            };


            
            
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
