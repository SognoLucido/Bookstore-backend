using Database.Model;
using Database.Model.Apimodels;
using Database.Model.ModelsDto;
using Database.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;


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
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<DetailedFilterBookModel>))]
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
        /// <param name="data">
        /// "role" : "admin" or "user" <br />
        /// email or userID
        /// </param>
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
        public async Task<IActionResult> Changerole([FromBody] Rolechanger data)
        {
            if (!ModelState.IsValid) return BadRequest(data);
            if (data.UserID is null && data.email is null) return BadRequest("At least one userid or email, is required");


            UserRole role;

            switch (data.Role.ToLower())
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



        /// <remarks>
        /// Sample request:
        /// 
        ///    Author name and category MUST match the existing records
        ///    
        /// </remarks>
        [HttpPost]
        [Route("book")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(List<DetailedFilterBookModel>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest,Type = typeof(Respostebookapi))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(Respostebookapi))]
        public async Task<IActionResult> InsertBook([FromBody][Required] List<BookinsertModel?> bodydata) 
        {

            if (bodydata.Count  == 0 ) return BadRequest();

            foreach (var item in bodydata)
            {
                if (item.PublicationDate.year > DateTime.UtcNow.Year) return BadRequest("wrong year");
            }

        

            var (booklist,ErrStatuscode) = await dbcall.InsertBooksItem(bodydata);


            return booklist is null ? StatusCode(ErrStatuscode.Code, ErrStatuscode) : StatusCode(201, booklist);
            




        }


        /// <summary>
        /// Upsert a list of authors or a list of categories in the database
        /// </summary>

        /// <param name="AuthorUpinsert">
        /// - <c>True</c>: Content in the Author that matches existing records in the database will be updated(bio) as provided, and non-existent records will be added .
        /// - <c>False/default(--)</c>: Add only /the non-existent record/s ,
        /// </param>
        /// <returns></returns>
        [HttpPost] 
        [Route("upsertINFO")]
        public async Task<IActionResult> InsertCategoryxAuthor([FromBody] CategoryandAuthorDto body , bool AuthorUpinsert)
        {

            if (!ModelState.IsValid) return BadRequest();
            if (body.Category is null && body.Author is null) return BadRequest();

         return await dbcall.UpinsertAuthorsxCategories(body,AuthorUpinsert) == true ? Ok() : BadRequest() ;

           

        }






        /// <summary>
        /// 
        /// </summary>

        /// <param name="ForceOverride">If true, the qnty query will override the current stock in the database; otherwise, Dbstocktotal will be incremented by qnty</param>
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
        public async Task<IActionResult> Changeprice(
            [FromRoute][MaxLength(30)][RegularExpression("^[0-9]*$")] string ISBN,
            [FromQuery] decimal price,
            CancellationToken ctoken)
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



        /// <summary>
        /// Delete a user account by User ID or email. (Admins can delete user accounts but cannot delete other admin accounts.)
        /// </summary>
        /// <returns></returns>
        [HttpDelete]
        [Route("account")]
        public async Task<IActionResult> DeleteAccount
            (
            [FromQuery] Guid? userid,
            [FromQuery][EmailAddress] string? email
            )
        {
            if (userid is null && email is null)return BadRequest();   
            if (!User.HasClaim("ruoli", "admin")) return BadRequest();




            if(userid is not null)
            {
                if (await dbcall.DeleteAccount(userid)) return Ok("Deletion was successful");
                else return NotFound();
            }
            else if (email is not null) 
            {
                if (await dbcall.DeleteAccount(email)) return Ok("Deletion was successful");
                else return NotFound();
            }


            return StatusCode(500);
        }









    }
}
