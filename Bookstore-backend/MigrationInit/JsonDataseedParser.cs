using Bookstore_backend.MigrationInit.Model;
using System.Reflection.Metadata.Ecma335;
using System.Text.Json;

namespace Bookstore_backend.MigrationInit
{
    public class JsonDataseedParser
    {


        public Booksdataseed? BooksDeserialize()
        {
            string jsonString = File.ReadAllText("MigrationInit\\dataseed.json");
            var x = JsonSerializer.Deserialize<Booksdataseed>(jsonString);
            return x;
        }   

        public Admindataseed? AdminDeserialize()
        {
            //string jsonString = File.ReadAllText("MigrationInit\\test.json");
            // string jsonString = File.ReadAllText("MigrationInit\\Enumt.json");


            string jsonString = File.ReadAllText("MigrationInit\\dataseedAdminAccount.json");
            var jsondata = JsonSerializer.Deserialize<Admindataseed>(jsonString);

            if (jsondata is null) return null;

            jsondata.Adminapidata.Apikey = Guid.NewGuid() ;

            return jsondata;
            // var seedData = JsonSerializer.Deserialize<EnumTest>(jsonString);
        }




    }
}
