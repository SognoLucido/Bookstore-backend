using Database.Model;
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
    public class AdminController : ControllerBase
    {

        private readonly ICrudlayer dbcall;
        public AdminController(ICrudlayer crud)
        {
            dbcall = crud;

        }


        [HttpPost]
        //[Authorize("AdminOnly")]
        [Route("changerole")]
        public async Task<IActionResult> Changerole ([FromBody] Rolechanger data)
        {
            if (!ModelState.IsValid) return BadRequest(data);

            UserRole role;

            switch(data.Role.ToLower())
            {
                case "admin": role = UserRole.admin; break;
                case "user": role = UserRole.user; break;

                default: return BadRequest("Invalid role.  choose one: admin , user");
            }


            if (await dbcall.ChangeRoles(data.UserID, data.email, role)) return Ok();

            return NotFound();

        }





        [HttpPost]
        //[Authorize("AdminOnly")]
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

        [HttpPatch("bookstock/{ISBN}")]
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

        [HttpPatch("bookprice/{ISBN}")]
        //[Authorize("AdminOnly")]
        public async Task<IActionResult> Changeprice([FromRoute] string ISBN, [FromQuery] decimal price,CancellationToken ctoken)
        {

            //
            if (await dbcall.Pricebookset(ISBN, price, ctoken))  return Ok();
           

            return StatusCode(500, "update failed");
           
        }



        [HttpDelete("book/{ISBN}")]
        [Authorize("AdminOnly")]
        public async Task<IActionResult> DeletebyISBN([FromRoute][RegularExpression("^[0-9]*$")] string ISBN, CancellationToken cToken)
        {

            if (await dbcall.Deletebyisbn(ISBN, cToken))
            {
                return Ok();
            }
            return NotFound();


        }

   

        [HttpDelete]
        [Authorize]
        [Route("account")]
        public async Task<IActionResult> DeleteAccount([FromQuery] Guid? userid, [FromQuery][EmailAddress] string? email,CancellationToken ctoken)
        {
        

            (Guid? UserdbGuid, string Role) = (null,string.Empty);

            foreach (var claims in User.Claims)
            {

                switch(claims.Type)
                {
                    case "UserID": 
                        {
                            if (claims.Value.IsNullOrEmpty()) return BadRequest();
                            else UserdbGuid = Guid.Parse(claims.Value);
                        }; break;
                    case "ruoli":
                        {
                            if (claims.Value.IsNullOrEmpty()) return BadRequest();
                            else Role = claims.Value;
                        }; break;

                }

            }

            if(Role == "user" && userid != UserdbGuid) return Unauthorized();
            else if(Role == "admin" && userid == UserdbGuid )return BadRequest();



            switch (Role)
            {
                case "user": 
                    {
                        if (await dbcall.DeleteAccount(UserdbGuid)) return Ok();

                    };break;

                case "admin": 
                    {
                        if(email is not null)
                        {
                            if(await dbcall.DeleteAccount(email)) return Ok();                          
                        }
                        else
                        {
                            if (await dbcall.DeleteAccount(UserdbGuid)) return Ok();
                           
                        }

                    };break;

            };

          
            return StatusCode(500, "delete failed");
        }



    








    }
}
