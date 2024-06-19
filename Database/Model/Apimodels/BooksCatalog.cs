﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Model.Apimodels
{
    public class BooksCatalog
    {

        public string ISBN { get; set; }
        public string BookTitle { get; set; }

        public string Category { get; set; }

        public string Price { get; set; }

        public string Description { get; set; }

        public string AuthorName { get; set; }
    }
}
