using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Model
{

   
    public class RolesModel
    {
        public int Id { get; set; } 

        public string Roles { get; set; }

        public List<Customer> Customer { get; set; }
        


    }
}
