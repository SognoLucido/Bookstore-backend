using Database.Services;
using Microsoft.AspNetCore.Mvc;
using Database.ApplicationDbcontext;
using Database.Model;
using System.Data.Common;
// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Bookstore_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestapiController : ControllerBase
    {
        

        


        //// GET: api/<testapi>
        //[HttpGet]
        //public IEnumerable<string> Get()
        //{
        //    return new string[] { "value1", "value2" };
        //}

        // GET api/<testapi>/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Person>> Get(int id, [FromServices]ICrudlayer dbconn)
        {

          return await dbconn.Getbyid(id);
          
        }


        [HttpPost]
        public async Task Post([FromBody] Person person, [FromServices] ICrudlayer dbconn)
        {
          await dbconn.Insert(person);    

        }

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
}
