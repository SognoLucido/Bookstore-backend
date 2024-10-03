using Bookstore.Api.Integration.test.Model;
using Bookstore_backend.MigrationInit.Model;
using System.Text.Json;


namespace Bookstore.Api.Integration.test.Dataseed
{
    public class JsonDataseedParsertest
    {

        public Booksdataseed? DataDeserialize()
        {
            string filePath = Path.Combine("Dataseed", "Testbooksdata.json");
            string jsonString = File.ReadAllText(filePath);

            try
            {
                return JsonSerializer.Deserialize<Booksdataseed>(jsonString);
                
            }
            catch (JsonException )
            {
                return null;
            }


        }

     

    }




}
