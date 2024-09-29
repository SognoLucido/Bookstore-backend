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
            //For default accounts, like the user we created earlier (TestUser), the limit for the API key service is 5 calls every 3 minutes (hardcoded) L180 DbBookCrud.cs
            //We are testing the limit to ensure everything works correctly 
            for (int i = 0; i < 7; i++)
            {
                _ = await _client.GetAsync($"apikey/{Fakebook.ISBN}");
            }

            var SubscriptionServiceAccountCap = await _client.GetAsync($"apikey/{Fakebook.ISBN}");


            //////////////////////


            Assert.Equal(HttpStatusCode.OK, ApikeygetTest.StatusCode);
            Assert.Equal(Fakebook.ISBN, ISBNextractMatch.Result.isbn);
            Assert.Equal(HttpStatusCode.NotFound, WrongISBN.StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, WrongApikey.StatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, MisspelledApiKey.StatusCode);
            Assert.Equal(HttpStatusCode.TooManyRequests, SubscriptionServiceAccountCap.StatusCode);



        }




        [Fact]
        public async Task Public_search_endpoint()
        {



            var _client = _factory.CreateClient();
            var AdminCredentials = new Login()
            {
                Email = "admin@example.com",
                Password = "admin"
            };
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


            ////////////////////
            
            var AdminTokenBody = await _client.PostAsJsonAsync("auth/login", AdminCredentials);
            var AdminTokenraw = AdminTokenBody.Content.ReadFromJsonAsync<Tokenlogin>();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AdminTokenraw.Result.result);

            _ = await _client.PostAsJsonAsync("api/upsertINFO?AuthorUpinsert=true", AuthorAndCategory);
            _ = await _client.PostAsJsonAsync("api/book", Fakebook);

            _client.DefaultRequestHeaders.Authorization = null;

            var FindBookTile = await _client.GetAsync("/api/search?booktitle=test");
            var NonexistentBookTitle = await _client.GetAsync("/api/search?booktitle=awdwdfw");


            ///////////////////



            Assert.Equal(HttpStatusCode.OK, FindBookTile.StatusCode);
            Assert.Equal(HttpStatusCode.NotFound,NonexistentBookTitle.StatusCode);
        }











        [Fact]
        public async Task Public_booklistPage_endpoint()
        {
            var _client = _factory.CreateClient();

            ////////////
           
            var getList = await _client.GetFromJsonAsync<List<BooksCatalog>>("/api/booklist?Page=1&Pagesize=4");

            //////////
            Assert.Equal(4, getList.Count());

        }







        }
    }
