using Bookstore.Api.Integration.test.Model;
using Database.Model.Apimodels;
using Database.Model.ModelsDto;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Bookstore.Api.Integration.test
{
    public class BookStorePublicEndpointsTests : IClassFixture<ProgramTestApplicationFactory>
    {

        private readonly ProgramTestApplicationFactory _factory;

        public BookStorePublicEndpointsTests(ProgramTestApplicationFactory factory)
        {
            _factory = factory;
        }





        //














        [Fact]
        public async Task Public_apikey_service()
        {
            var _client = _factory.CreateClient();
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
            //This record (admin account) is automatically injected during database creation. The values are known; check MigrationInit in the main project
            var AdminCredentials = new Login()
            {
                Email = "admin@example.com",
                Password = "admin"
            };
            // The api_key will consume a service where the ISBN of the book will be used as a query parameter, so we create a fake book and insert it into the database before verifying the API key logic (object creation below)
            // Since we cannot insert the book without an author and category, we create those as well. 
            var AuthorAndCategory = new CategoryandAuthorDto()
            {
                Author =
                [
                      new() { FullName = "testauthor", Bio = "bio" }
                ],
                Category =
                [
                     new() { Name = "testcategory" }
                ]
            };
            var Fakebook = new BookinsertModel
            {
                Title = "test",
                Author = AuthorAndCategory.Author[0].FullName,
                Category = AuthorAndCategory.Category[0].Name,
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



            /////////////////////

            var InsertUserBody = await _client.PostAsJsonAsync("auth/register", TestUser);
            var UserTokenBody = await _client.PostAsJsonAsync("auth/login", UserCredentials);
            var AdminTokenBody = await _client.PostAsJsonAsync("auth/login", AdminCredentials);

            var UserTokenraw = UserTokenBody.Content.ReadFromJsonAsync<Tokenlogin>();
            var AdminTokenraw = AdminTokenBody.Content.ReadFromJsonAsync<Tokenlogin>();

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AdminTokenraw.Result.result);
            _ = await _client.PostAsJsonAsync("api/upsertINFO?AuthorUpinsert=true", AuthorAndCategory);
            _ = await _client.PostAsJsonAsync("api/book", Fakebook);

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", UserTokenraw.Result.result);
            var UserAccountInfo = await _client.GetAsync("api/userinfo");
            var UserApikey = await UserAccountInfo.Content.ReadFromJsonAsync<UserInfoOnlykey>();

            _client.DefaultRequestHeaders.Authorization = null;
            _client.DefaultRequestHeaders.Add("x-api-key", UserApikey.apiInfo.apikey);

            var ApikeygetTest = await _client.GetAsync($"apikey/{Fakebook.ISBN}");
            var ISBNextract = ApikeygetTest.Content.ReadFromJsonAsync<Isbn>();

            var TestWrongISBN = await _client.GetAsync("apikey/9999999999999");
            //For default accounts, like the user we created earlier (TestUser), the limit for the API key service is 5 calls every 3 minutes (hardcoded) L180 DbBookCrud.cs
            //We are testing the limit to ensure everything works correctly 
            for (int i = 0; i < 7; i++)
            {
                _ = await _client.GetAsync($"apikey/{Fakebook.ISBN}");
            }

            var Finalresult = await _client.GetAsync($"apikey/{Fakebook.ISBN}");


            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add("x-api-key", Guid.NewGuid().ToString("N"));
            var TestWrongApikey = await _client.GetAsync($"apikey/{Fakebook.ISBN}");


            //TODO FIX
            // BUG: If the ISBN is wrong and the API key is correct, the service returns a 200 status code (should be 404).
            // Currently, it returns status 200 && JSON: { "code": 200, "message": null }.


            //////////////////////


            Assert.Equal(HttpStatusCode.OK, ApikeygetTest.StatusCode);
            Assert.Equal(Fakebook.ISBN, ISBNextract.Result.isbn);
            Assert.Equal(HttpStatusCode.TooManyRequests, Finalresult.StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, TestWrongApikey.StatusCode);




        }
    }
}
