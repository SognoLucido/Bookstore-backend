using Auth;
using Database.Model.Apimodels;
using Database.Services;
using Microsoft.AspNetCore.Mvc;




namespace Bookstore_backend.Controllers
{
    [Route("auth/")]
    [ApiController]
    public class AuthController(ICrudlayer _dbcall) : ControllerBase
    {

        private readonly ICrudlayer dbcall = _dbcall;

        [HttpPost]
        [Route("register")]   
        public async Task<IActionResult> Register([FromBody] Registration regi, CancellationToken token)
        {
            if (!ModelState.IsValid) return UnprocessableEntity(ModelState);


            if (await dbcall.Registration(regi, token))
            {
                return Ok();
            }
            else return BadRequest("User already exist");
            // check if already exyst by email 
            //register

        }

        /// <summary>
        /// adminAccount :  email -> admin@example.com , passw -> admin
        /// </summary>
        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] Login login,[FromServices] ITokenService tokengen, CancellationToken ctoken)
        {
            if (!ModelState.IsValid) return UnprocessableEntity(ModelState);

            var data = await dbcall.Login(login, ctoken);

            if (data.Item1 is not null && data.Item2 is not null)
            {
                return Ok(tokengen.GenerateToken(data.Item1,data.Item2));
            }
            else
            {
                return Unauthorized("Invalid Email or passwd");
            }

        }
    }
}
