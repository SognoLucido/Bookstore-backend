﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Model.Apimodels
{
    public class Registration
    {

        [MaxLength(30)]
        public string FirstName { get; set; }


        [MaxLength(30)]
        public string LastName { get; set; }

        [MaxLength(40)]
        [EmailAddress]
        public string Email { get; set; }

        [MaxLength(64)]
        public string Password { get; set; }
 

        [MaxLength(40)]
        public string Address { get; set; }

        [MaxLength(20)]
        public string? Phone { get; set; }
    }
}
