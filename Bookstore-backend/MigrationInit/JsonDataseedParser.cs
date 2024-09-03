using Bookstore_backend.MigrationInit.Model;
using System.Text.Json;

namespace Bookstore_backend.MigrationInit
{
    public class JsonDataseedParser
    {


        public Booksdataseed? BooksDeserialize()
        {
           
            string filePath = Path.Combine("MigrationInit", "dataseed.json");
            string jsonString = File.ReadAllText(filePath);
            var x = JsonSerializer.Deserialize<Booksdataseed>(jsonString);
            return x;
        }   

        public Admindataseed? AdminDeserialize()
        {

            string filePath = Path.Combine("MigrationInit", "dataseedAdminAccount.json");
            string jsonString = File.ReadAllText(filePath);
            var jsondata = JsonSerializer.Deserialize<Admindataseed>(jsonString);

            if (jsondata is null) return null;

            jsondata.Adminapidata.Apikey = Guid.NewGuid() ;

            return jsondata;
           
        }




    }
}
