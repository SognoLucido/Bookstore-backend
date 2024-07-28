using Database.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using Database.Model.ModelsDto.Paymentmodels;
using Database.Model.ModelsDto;

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
        public async Task<IActionResult> AccountInfoGet()
        {
            var UserID = User.Claims.SingleOrDefault(x => x.Type == "UserID");

            if (!Guid.TryParse(UserID.Value, out Guid userIDokcheck)) return BadRequest();


            var dbdata = await dbcall.GetUserInfoAccount(userIDokcheck);


            return dbdata is not null ? Ok(dbdata) : NotFound("invalid UserID - user not found");



            //return Ok(await dbcall.GetUserInfoAccount(Guid.Parse("815edd39-8264-4dba-a76d-833aff20fd7f")));
        }





        [HttpDelete]
        [Authorize]
        [Route("account")]
        public async Task<IActionResult> DeleteAccount([FromQuery] Guid? userid, [FromQuery][EmailAddress] string? email, CancellationToken ctoken)
        {


            (Guid? UserdbGuid, string Role) = (null, string.Empty);

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
                            else Role = claims.Value;
                        }; break;

                }

            }

            if (Role == "user" && userid != UserdbGuid) return Unauthorized();
            else if (Role == "admin" && userid == UserdbGuid) return BadRequest();



            switch (Role)
            {
                case "user":
                    {
                        if (await dbcall.DeleteAccount(UserdbGuid)) return Ok();

                    }; break;

                case "admin":
                    {
                        if (email is not null)
                        {
                            if (await dbcall.DeleteAccount(email)) return Ok();
                        }
                        else
                        {
                            if (await dbcall.DeleteAccount(UserdbGuid)) return Ok();

                        }

                    }; break;

            };


            return StatusCode(500, "delete failed");
        }



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
