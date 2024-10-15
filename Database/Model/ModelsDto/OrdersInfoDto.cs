using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Model.ModelsDto
{
    public class OrdersInfoDto
    {

        public Guid userid { get; set; }

        public  Orderchild[] orders {  get; set; }

    }

  

    public record Orderchild
        (
            DateTime Date ,
            int id,
            string status,
            List<OrderinfoBaseCut> order,
            float total
        );


    public record OrderinfoBaseCut
        (
            string ISBN,
            string Title,
            decimal Price,
            int Qnty

        );

}
