using Database.Model;
using Database.Model.Apimodels;
using Database.Model.ModelsDto;
using Database.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata.Ecma335;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Bookstore_backend.Controllers
{
    [Route("api/")]
    [ApiController]
    [Authorize("AdminOnly")]
    public class AdminController : ControllerBase
    {

        private readonly ICrudlayer dbcall;
        public AdminController(ICrudlayer crud)
        {
            dbcall = crud;

        }



        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="info">0 get Authors // 1 get Categories </param>
        ///// <param name="limit"> Limit the number of batches . if null return list-all </param>
        ///// <param name="searchbyname"> optional  </param>
        ///// <returns></returns>
        //[HttpGet]
        //[Route("iteminfo")]
        //public async Task<IActionResult> Getinfo(Info info, [FromQuery] int? limit, [FromQuery][MaxLength(20)] string? searchbyname)
        //{
        //    if (limit <= 0 ) return BadRequest();

        //    if (info == Info.Authors)
        //    {
        //        return Ok(await dbcall.GetAuthorinfo(limit,searchbyname));
        //    }
        //    else
        //    {
        //        return Ok(await dbcall.GetCategoriesinfo(limit,searchbyname));
        //    }


        //}


        /// <summary>
        /// substring matching no limit 
        /// </summary>
        /// <param name="booktitle">optional</param>
        /// <param name="authorname">opt</param>
        /// <param name="category">opt</param> 
        /// <param name="limit">opt</param> 
        /// <returns></returns>
        [HttpGet]
        [Route("admin/search")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(DetailedFilterBookModel))]
        public async Task<IActionResult> SearchItems(
            [FromQuery] string? booktitle,
            [FromQuery] string? authorname,
            [FromQuery] string? category,
            [FromQuery] int? limit,
            CancellationToken cToken)
        {  

            var data = (booktitle, authorname, category);
  

            var test = await dbcall.SearchItems(limit, data, cToken);

            return Ok(test);

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="data">"role" : "admin" or "user"</param>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST /
        ///     {
        ///         "email" : "user2@example.com"
        ///         "role" : "admin"
        ///     }
        ///    
        /// </remarks>
        /// <returns></returns>
        [HttpPost]
        [Route("changerole")]
        public async Task<IActionResult> Changerole ([FromBody] Rolechanger data)
        {
            if (!ModelState.IsValid) return BadRequest(data);
            if( data.UserID is null && data.email is  null )return BadRequest("At least one userid or email, is required");


            UserRole role;

            switch(data.Role.ToLower())
            {
                case "admin": role = UserRole.admin; break;
                case "user":
                    {
                        if (data.UserID == Guid.Parse("8233a0ab-78ac-4ee7-916f-0cbb93e85a63") || data.email == "admin@example.com") return BadRequest();
                       
                        role = UserRole.user;
                    } break;

                default: return BadRequest("Invalid role.  choose one: admin , user");
            }


            if (await dbcall.ChangeRoles(data.UserID, data.email, role)) return Ok();

            return NotFound();

        }

        /// <summary>
        /// Upgrade or downgrade the API key tier; no payment is involved. Admin access required
        /// </summary>

        //[HttpPost]
        //[Route("upgradekey")]
        //public async Task<IActionResult> Updatetier([FromQuery] )
        //{



        //    return Ok();
        //}


        /// <remarks>
        /// Sample request:
        /// 
        ///    Author name and category MUST match the existing records
        ///    
        /// </remarks>
        [HttpPost]     
        [Route("book")]
        public async Task<IActionResult> InsertBook([FromBody] BookinsertModel bodydata)
        {

            if (!ModelState.IsValid) return BadRequest(bodydata);

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




            return StatusCode(message.Code, message);

            //switch (message.Code)
            //{
            //    case 200: return Ok(message.Message);
            //    case 500: return StatusCode(500, message.Message);
            //    case 404: return NotFound(message.Message);
            //    case 409: return StatusCode(409, message.Message);

            //    default: return BadRequest();
            //}



        }
        /// <summary>
        /// 
        /// </summary>
        
        /// <param name="ForceOverride">if true {the "qnty" query will override the current stock in the db } else Dbstocktotal += "qnty" </param>
        /// <returns></returns>
        [HttpPatch("bookstock/{ISBN}")]
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

        [HttpPatch("bookprice/{ISBN}")]
     
        public async Task<IActionResult> Changeprice([FromRoute] string ISBN, [FromQuery] decimal price,CancellationToken ctoken)
        {

            //
            if (await dbcall.Pricebookset(ISBN, price, ctoken))  return Ok();
           

            return StatusCode(500, "update failed");
           
        }



        [HttpDelete("book/{ISBN}")]
     
        public async Task<IActionResult> DeletebyISBN([FromRoute][RegularExpression("^[0-9]*$")] string ISBN, CancellationToken cToken)
        {

            if (await dbcall.Deletebyisbn(ISBN, cToken))
            {
                return Ok();
            }
            return NotFound();


        }

   


    








    }
}
