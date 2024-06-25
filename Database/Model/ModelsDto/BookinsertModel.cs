using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Database.Model.ModelsDto
{
    public class BookinsertModel
    {
        public string Title { get; set; } 
        public string Author { get; set; }
        public string Category { get; set; }

        
        [MaxLength(13)]
        [RegularExpression("^[0-9]*$")]
        public string ISBN { get; set; }
        public decimal Price { get; set; }

        public int StockQuantity { get; set; }
       
        public  Dateclass PublicationDate { get; set; }

        public string Description { get; set; }

       

        
    }


    public class Dateclass
    {



        [Range(0, 4000)]
        public int year { get; set; }

        [Range(1,12)]
        public int month { get; set; }


        [Range(1, 31)]
        public int day { get; set; }


        //public Double Maxyeartoday => DateTime.UtcNow.Year;



    }

    

}
