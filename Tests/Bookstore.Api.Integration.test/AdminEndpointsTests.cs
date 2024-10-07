using Bookstore.Api.Integration.test.Dataseed;
using Bookstore.Api.Integration.test.Model;
using Database.Model;
using Database.Model.Apimodels;
using Database.Model.ModelsDto;
using Microsoft.Extensions.DependencyInjection;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Bookstore.Api.Integration.test
{
    public class AdminEndpointsTests(ProgramTestApplicationFactory _factory) : IClassFixture<ProgramTestApplicationFactory>, IAsyncLifetime
    {

        private readonly HttpClient client = _factory.CreateClient();
        private readonly DataseedperTestLogic seed = _factory.Services.GetRequiredService<DataseedperTestLogic>();


        [Fact]
        public async Task AdminSearch_books()
        {
            var adminCredentials = new Login()
            {
                Email = "admin@example.com",
                Password = "password123"
            };

            await seed.BaseDatabookseed();
            await seed.BaseDatabookseed();
            await seed.InsertDummyuser(adminCredentials, UserRole.admin);

            //////////////////////

            var adminTokenBody = await client.PostAsJsonAsync("auth/login", adminCredentials);
            var adminTokenraw = adminTokenBody.Content.ReadFromJsonAsync<Tokenlogin>();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminTokenraw.Result.result);

            var Getallbooks = await client.GetFromJsonAsync<List<DetailedFilterBookModel>>("/api/admin/search");

            // The normal API book search is hardcoded with a limit of 5 items per call, but the admin version (which we are testing) is unlimited
            // We inserted 10 books (await seed.BaseDataBookSeed() x2), so we expect all 10 books
            int Bookscount = Getallbooks?.Count ?? 0;
            //////////////////////


            Assert.Equal(10, Bookscount);


        }








        [Fact]
        public async Task ChangeRole()
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var AdminCredentials = new Login()
            {
                Email = "admin@example.com",
                Password = "password123"
            };
            var UserCredentials = new Login()
            {
                Email = "user@example.com",
                Password = "password123"
            };


            await seed.InsertDummyuser(AdminCredentials, UserRole.admin);
            await seed.InsertDummyuser(UserCredentials);


            var userTokenBody = await client.PostAsJsonAsync("auth/login", UserCredentials);
            var userTokenraw = userTokenBody.Content.ReadFromJsonAsync<Tokenlogin>();

            var adminTokenBody = await client.PostAsJsonAsync("auth/login", AdminCredentials);
            var adminTokenraw = adminTokenBody.Content.ReadFromJsonAsync<Tokenlogin>();


            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userTokenraw.Result.result);
            var getUserRole = await client.GetFromJsonAsync<Role>("api/userinfo");

            // client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminTokenraw.Result.result);
            // The "api/changerole" endpoint has two options for changing roles: by email and by userId; testing the email one below
            var changeRole = await client.PostAsJsonAsync("api/changerole", new RolebyEmail(UserCredentials.Email, "admin"));

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userTokenraw.Result.result);
            var getUserRoleChangedToAdmin = await client.GetFromJsonAsync<Role>("api/userinfo");

            var UserToken = tokenHandler.ReadJwtToken(userTokenraw.Result.result);
            var UserClaimUserID = UserToken.Claims.First(x => x.Type == "UserID").Value;

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminTokenraw.Result.result);
            //by UserID
            var changeRolebyuserID = await client.PostAsJsonAsync("api/changerole", new RolebyUserID(UserClaimUserID, "user"));

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userTokenraw.Result.result);
            var getUserRolechangedBacktoUser = await client.GetFromJsonAsync<Role>("api/userinfo");

            ////////////////

            Assert.Equal("user", getUserRole.role);
            Assert.Equal(HttpStatusCode.OK, changeRole.StatusCode);
            Assert.Equal("admin", getUserRoleChangedToAdmin.role);
            Assert.Equal(HttpStatusCode.OK, changeRolebyuserID.StatusCode);
            Assert.Equal("user", getUserRolechangedBacktoUser.role);


        }




        [Fact]
        public async Task Insert_BookTest()
        {
            var AdminCredentials = new Login()
            {
                Email = "admin@example.com",
                Password = "password123"
            };

            string AuthorNameToinsert = "TestAuthor";
            string CategoryNameToinsert = "TestCategory";

            var FakebookList = new List<BookinsertModel>
            {
                new()
                {
                Title = "testtitleyoz",
                Author =  AuthorNameToinsert,
                Category = CategoryNameToinsert,
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
                 }
            };


            await seed.InsertDummyuser(AdminCredentials, UserRole.admin);
            await seed.InsertCustomAuthorCategory(AuthorNameToinsert, CategoryNameToinsert);

            //////////////////////

            var adminTokenBody = await client.PostAsJsonAsync("auth/login", AdminCredentials);
            var adminTokenraw = adminTokenBody.Content.ReadFromJsonAsync<Tokenlogin>();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminTokenraw.Result.result);

            var insertBook = await client.PostAsJsonAsync("api/book", FakebookList);
            var getbook = await client.GetFromJsonAsync<List<DetailedFilterBookModel>>($"/api/admin/search?booktitle={FakebookList[0].Title}");

            //book already exist
            var insertBooktest2 = await client.PostAsJsonAsync("api/book", FakebookList);

            //////////////////////

            Assert.Equal(HttpStatusCode.Created, insertBook.StatusCode);
            Assert.True(getbook[0].BookTitle == FakebookList[0].Title &&
                        getbook[0].AuthorName == AuthorNameToinsert.ToLower() &&
                        getbook[0].Category == CategoryNameToinsert.ToLower());
            Assert.Equal(HttpStatusCode.BadRequest, insertBooktest2.StatusCode);




        }





        public async Task DisposeAsync()
        {
            await seed.CleanUser();
        }

        public Task InitializeAsync()
        {
            return Task.CompletedTask;
        }
    }
}
