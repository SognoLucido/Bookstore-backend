using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Model.ModelsDto
{
    public class AuthorDto
    {

        public int AuthorId { get; set; }

        public string FullName { get; set; }
        public string Bio { get; set; }

        public List<Books> Books { get; set; }
    }


    public class Books
    {
        public string book {  get; set; }
    }


    


}
