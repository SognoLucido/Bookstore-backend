using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Database.Model.ModelsDto
{
    public class Rolechanger
    {
        [EmailAddress]
        public string? email {  get; set; }

        
        public Guid? UserID { get; set; }

        [MaxLength(20)]
        public string Role {  get; set; }


    }
}
