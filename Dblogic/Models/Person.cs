using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dblogic.Models
{
   public class Person
    {

        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(40)]
        public string Name { get; set; }

        [Required]
        [StringLength(40)]
        public string LastName { get; set; }

        [Required]
        public int Age { get; set; }


    }
}
