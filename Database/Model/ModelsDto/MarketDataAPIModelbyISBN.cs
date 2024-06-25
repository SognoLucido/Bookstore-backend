using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Model.Apimodels
{
    public class MarketDataAPIModelbyISBN
    {
        public string ISBN { get; set; }

        public string Price { get; set; }

        public int StockQuantity { get; set; }

    }
}
