using Bookstore.Api.Integration.test.Dataseed;
using Bookstore.Api.Integration.test.Model;
using Database.Model.Apimodels;
using Database.Model.ModelsDto;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Bookstore.Api.Integration.test.EndpointsTests
{
    public class BookStorePublicEndpointsTests(ProgramTestApplicationFactory _factory) : IClassFixture<ProgramTestApplicationFactory>, IAsyncLifetime
    {

        private readonly ProgramTestApplicationFactory factory = _factory;
        private readonly DataseedperTestLogic seed = _factory.Services.GetRequiredService<DataseedperTestLogic>();

        [Fact]
        public async Task Public_apikey_service()
        {
            var _client = factory.CreateClient();
            var TestUser = new Registration
            {
                FirstName = "test",
                LastName = "user",
                Email = "userv1@example.com",
                Password = "password123",
                Address = "string",
                Phone = "string"

            };
            var UserCredentials = new Login()
            {
                Email = TestUser.Email,
                Password = TestUser.Password,
            };
            var Fakebook = new BookinsertModel
            {
                Title = "test",
                ISBN = "0000000000101",
                Price = 10,
                StockQuantity = 100,
                PublicationDate = new()
                {
                    year = 2000,
                    month = 1,
                    day = 1,
                },
                Description = "test",

            };

            await seed.InsertCustombook(Fakebook);

            /////////////////////

            var InsertUserBody = await _client.PostAsJsonAsync("auth/register", TestUser);
            var UserTokenBody = await _client.PostAsJsonAsync("auth/login", UserCredentials);

            var UserTokenraw = UserTokenBody.Content.ReadFromJsonAsync<Tokenlogin>();

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", UserTokenraw.Result.result);
            var UserApikey = await _client.GetFromJsonAsync<UserInfoOnlykey>("api/userinfo");

            _client.DefaultRequestHeaders.Authorization = null;
            _client.DefaultRequestHeaders.Add("x-api-key", UserApikey.apiInfo.apikey);

            var ApikeygetTest = await _client.GetAsync($"apikey/{Fakebook.ISBN}");
            var ISBNextractMatch = ApikeygetTest.Content.ReadFromJsonAsync<Isbn>();

            var WrongISBN = await _client.GetAsync("apikey/9999999999999");

            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add("x-api-key", Guid.NewGuid().ToString("N")); // Will query the db to check if the key exist
            var WrongApikey = await _client.GetAsync($"apikey/{Fakebook.ISBN}");

            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add("x-api-key", "29udad"); // Request blocked before reaching the database due to an invalid format key
            var MisspelledApiKey = await _client.GetAsync($"apikey/{Fakebook.ISBN}");

            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add("x-api-key", UserApikey.apiInfo.apikey);
            //For default accounts, like the user we created earlier (TestUser), the limit for the API key service is 5 calls every 3 minutes (hardcoded) 
            //We are testing the limit to ensure everything works correctly 
            for (int i = 0; i < 7; i++)
            {
                _ = await _client.GetAsync($"apikey/{Fakebook.ISBN}");
            }

            var SubscriptionServiceAccountCap = await _client.GetAsync($"apikey/{Fakebook.ISBN}");


            //////////////////////


            Assert.Equal(HttpStatusCode.OK, ApikeygetTest.StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, WrongISBN.StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, WrongApikey.StatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, MisspelledApiKey.StatusCode);
            Assert.Equal(HttpStatusCode.TooManyRequests, SubscriptionServiceAccountCap.StatusCode);



        }




        [Fact]
        public async Task Public_search_endpoint()
        {

            var _client = factory.CreateClient();
            var Fakebook = new BookinsertModel
            {
                Title = "testtitleyoz",
                ISBN = "0000000000101",
                Price = 10,
                StockQuantity = 100,
                PublicationDate = new()
                {
                    year = 2000,
                    month = 1,
                    day = 1,
                },
                Description = "test",

            };
            await seed.InsertCustombook(Fakebook);



            ////////////////////

            var FindBookTile = await _client.GetAsync($"/api/search?booktitle={Fakebook.Title}");
            var NonexistentBookTitle = await _client.GetAsync("/api/search?booktitle=awdwdfw404");


            ///////////////////

            Assert.Equal(HttpStatusCode.OK, FindBookTile.StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, NonexistentBookTitle.StatusCode);
        }



        [Fact]
        public async Task Public_booklistPage_endpoint()
        {
            var _client = factory.CreateClient();


            /////////////

            var getList = await _client.GetFromJsonAsync<List<BooksCatalog>>("/api/booklist?Page=1&Pagesize=4");

            /////////////

            Assert.Equal(4, getList.Count);

        }

        public async Task InitializeAsync()
        {
            await seed.BaseDatabookseed();
        }

        public async Task DisposeAsync()
        {
            await seed.CleanBooks();

        }





    }
}
