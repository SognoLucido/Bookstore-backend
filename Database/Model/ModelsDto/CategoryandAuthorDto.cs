using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Model.ModelsDto
{
    public class CategoryandAuthorDto
    {

        public List<AuthorinsertDto?>? Author { get; set; }

        public List<CategoryinsertDto?>? Category { get; set; }
    }


    public class AuthorinsertDto
    {
        [MaxLength(30)]
        public string FullName { get; set; }
        public string Bio { get; set; }
    };

    public class  CategoryinsertDto
    {
        [MaxLength(30)]
        public string Name { get; set; }
    }



}
