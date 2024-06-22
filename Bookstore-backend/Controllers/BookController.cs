using Database.Services;
using Microsoft.AspNetCore.Mvc;
using Database.ApplicationDbcontext;
using Database.Model;
using System.Data.Common;
using Database;
using Database.Model.Apimodels;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

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
        [Authorize]
        public async  Task<ActionResult<BooksCatalog>> GetList([FromQuery] Pagemodel pagesettings,CancellationToken cToken)
        {

            if (!ModelState.IsValid )
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
        [Authorize("Userlogged")]
        public async Task<IActionResult> GetListfiltered([FromQuery] QuerySelector selector, CancellationToken cToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var data = await dbcall.Filteredquery(selector,cToken);

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
        public async Task<IActionResult> GetbyISBN([FromRoute][RegularExpression("^[0-9]*$")]string ISBN , CancellationToken cToken)
        {


            var data = await dbcall.Getbyisbn(ISBN,cToken);

            if(data is null)
            {
                return NotFound(data);
            }
            return Ok(data);



        }




        //[HttpPost]
        //[Route("Register")]
        //public async Task<IActionResult> Register([FromBody] Registration regi,CancellationToken token)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest();
        //    }


        //    if (await dbcall.Registration(regi,token))
        //    {
        //        return Ok();
        //    }
        //    else return BadRequest("User already exist");
        //    // check if already exyst by email 


        //    //register




        //}

        //[HttpPost]
        //[Route("Login")]
        //public async Task<IActionResult> Login([FromBody] Login login,CancellationToken token)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest();
        //    }

        //    if (await dbcall.Login(login,token))
        //    {
        //        return Ok();
        //    }
        //    else
        //    {
        //        return BadRequest("Invalid Email or passwd");
        //    }
           

           
        //    // check if already exyst by email 


        //    //register




        }



        //[HttpPost]
        //public async Task Post([FromBody] Person person, [FromServices] ICrudlayer dbconn)
        //{
        //  await dbconn.Insert(person);    

        //}

        //// PUT api/<testapi>/5
        //[HttpPut("{id}")]
        //public void Put(int id, [FromBody] string value)
        //{
        //}

        //// DELETE api/<testapi>/5
        //[HttpDelete("{id}")]
        //public void Delete(int id)
        //{
        //}







    
}
