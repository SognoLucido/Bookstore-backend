using Bookstore.Api.Integration.test.Dataseed;
using Bookstore.Api.Integration.test.Model;
using Database.Model;
using Database.Model.Apimodels;
using Database.Model.ModelsDto;
using Database.Model.ModelsDto.PaymentPartialmodels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualBasic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;



namespace Bookstore.Api.Integration.test.EndpointsTests
{


    [CollectionDefinition("Admincollection", DisableParallelization = true)]
    public class DatabaseCollection : IClassFixture<ProgramTestApplicationFactory>
    {

    }


    [Collection("Admincollection")]
    public class AdminEndpointsTests_Part3(ProgramTestApplicationFactory _factory) : IAsyncLifetime
    {

        private readonly HttpClient client = _factory.CreateClient();
        private readonly DataseedperTestLogic seed = _factory.Services.GetRequiredService<DataseedperTestLogic>();




        [Fact]
        public async Task Orders_details_info()
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var adminCredentials = new Login()
            {
                Email = "admin@example.com",
                Password = "password123"
            };

            var UserCredentials = new Login()
            {
                Email = "user@example.com",
                Password = "password123"
            };


            var FakebookList = new List<BookinsertModel>()
            {
                new()
                {

                Title = "test",
                ISBN = "0000000000100",
                Price = 10,
                StockQuantity = 10,
                PublicationDate = new()
                {
                    year = 2000,
                    month = 1,
                    day = 1,
                },
                Description = "test",
                },

                new()
                {
                Title = "test1",
                ISBN = "0000000000101",
                Price = 10,
                StockQuantity = 10,
                PublicationDate = new()
                {
                    year = 2000,
                    month = 1,
                    day = 1,
                },
                Description = "test",
                },

                new()
                {
                Title = "test2",
                ISBN = "0000000000102",
                Price = 10,
                StockQuantity = 10,
                PublicationDate = new()
                {
                    year = 2000,
                    month = 1,
                    day = 1,
                },
                Description = "test",
                }

               
            };


            const int QntyItem = 1;

            var bookBuyTemplatelist = new List<BookItemList>();

            foreach(var book in FakebookList)
            {
                bookBuyTemplatelist.Add(new BookItemList() { ISBN = book.ISBN, Quantity = QntyItem });
            }

            var bodyPurchase = new BookPartialPaymentModel
            {
                PaymentDetails = new()
                {
                    CardHolderName = "test",
                    CardNumber = "0000000000000000",
                    CardCVC = "000",
                    CardExpiry = new()
                    {
                        Month = 12,
                        Year = 80
                    }
                },
                BookItemList = bookBuyTemplatelist

            };



            await seed.InsertDummyuser(adminCredentials, UserRole.admin);
            await seed.InsertDummyuser(UserCredentials);
            await seed.BaseDatabookseed();
            await seed.InsertCustombook(FakebookList);


            /////////////////////////

            var UserTokenBody = await client.PostAsJsonAsync("auth/login", UserCredentials);
            var UserTokenraw = UserTokenBody.Content.ReadFromJsonAsync<Tokenlogin>();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", UserTokenraw.Result.result);


            var UserPurchase = await client.PostAsJsonAsync("api/buybook", bodyPurchase);

            var userToken = tokenHandler.ReadJwtToken(UserTokenraw.Result.result);
            var UserID = userToken.Claims.First(x => x.Type == "UserID").Value;

            var adminTokenBody = await client.PostAsJsonAsync("auth/login", adminCredentials);
            var adminTokenraw = adminTokenBody.Content.ReadFromJsonAsync<Tokenlogin>();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminTokenraw.Result.result);


            DateOnly currentDate = DateOnly.FromDateTime(DateTime.Now);


            var OrderInfoRequest = await client.GetAsync($"api/admin/orders-details?userid={UserID}&datestart={currentDate:MM,dd,yyyy}&dateend={currentDate:MM,dd,yyyy}");

            string jsonResponse = await OrderInfoRequest.Content.ReadAsStringAsync();

            var ordersDetails = JsonSerializer.Deserialize<OrdersInfoDto>(jsonResponse, options);

            var listofISBNinOrderInfoRequest = ordersDetails.orders[0].order.Select(s => s.ISBN).ToArray();
            var TotalpriceOrderitems = ordersDetails.orders[0].order.Sum(s => s.Price * s.Qnty);

            Console.WriteLine();
            /////////////////////////

            Assert.Equal(HttpStatusCode.OK, UserPurchase.StatusCode);

            Assert.Equal(UserID, ordersDetails.userid.ToString());

            Assert.Contains(FakebookList[0].ISBN,listofISBNinOrderInfoRequest);

            Assert.Equal(FakebookList.Sum(x => x.Price * QntyItem), TotalpriceOrderitems);





        }





        public async Task InitializeAsync()
        {
            await DisposeAsync();
        }

        public async Task DisposeAsync()
        {
            await seed.CleanBooks();
            await seed.CleanUser();
            await seed.CleanAuthorNCategory();
        }





    }

    [Collection("Admincollection")]
    public class AdminEndpointsTests_Part2(ProgramTestApplicationFactory _factory) : IAsyncLifetime
    {

        private readonly HttpClient client = _factory.CreateClient();
        private readonly DataseedperTestLogic seed = _factory.Services.GetRequiredService<DataseedperTestLogic>();



        [Fact]
        public async Task Insert_AuthorAndCategory()
        {



            //temp checking with the dbcontext Test 
            var AdminCredentials = new Login()
            {
                Email = "admin@example.com",
                Password = "password123"
            };
            var data = new CategoryandAuthorDto
            {
                Author =
                [
                    new() { FullName = "Author1test" , Bio = "string"},
                    new() { FullName = "Author2test" , Bio = "string"},
                    new() { FullName = "Author1test" , Bio = "string"}
                ],
                Category =
                [
                    new() { Name = "fantasy"},
                    new() { Name = "hello"},
                    new() { Name = "hello"}
                ]
            };

            await seed.InsertDummyuser(AdminCredentials, UserRole.admin);

            /////////////////////

            var adminTokenBody = await client.PostAsJsonAsync("auth/login", AdminCredentials);
            var adminTokenraw = adminTokenBody.Content.ReadFromJsonAsync<Tokenlogin>();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminTokenraw.Result.result);

            var insertdata = await client.PostAsJsonAsync("/api/upsertINFO", data);

            var dbdata = await seed.CheckAuthorNCategory();
            int totaladdedtodb = dbdata.Authors.Count + dbdata.Category.Count;

            //////////////////////


            Assert.Equal(HttpStatusCode.OK, insertdata.StatusCode);
            Assert.Equal(4, totaladdedtodb);
            Assert.Contains(data.Author[1].FullName, dbdata.Authors);
            Assert.Contains(data.Category[0].Name, dbdata.Category);


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




        public Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        public async Task DisposeAsync()
        {

            await seed.CleanUser();
            await seed.CleanAuthorNCategory();
        }
    }



    [Collection("Admincollection")]
    public class AdminEndpointsTests_Part1(ProgramTestApplicationFactory _factory) : IAsyncLifetime
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
        public async Task Search_singleItem()
        {

            var adminCredentials = new Login()
            {
                Email = "admintest@example.com",
                Password = "passwordzero"
            };

            const string AuthorName = "TestAuthorxz";
            const string CategoryName = "TestCategoryxz";


            await seed.InsertDummyuser(adminCredentials, UserRole.admin);
            await seed.InsertCustomAuthorCategory(AuthorName, CategoryName);



            ////////////////////


            var adminTokenBody = await client.PostAsJsonAsync("auth/login", adminCredentials);
            var adminTokenraw = adminTokenBody.Content.ReadFromJsonAsync<Tokenlogin>();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminTokenraw.Result.result);


            var GetUserInfo = await client.GetFromJsonAsync<Email>($"api/admin/searchitem?ItemName={adminCredentials.Email.ToLower()}&ItemType=userinfo");
            var GetAuthorName = await client.GetFromJsonAsync<FUllname>($"api/admin/searchitem?ItemName={AuthorName.ToLower()}&ItemType=author");
            var GetCategoryName = await client.GetFromJsonAsync<Categoryname>($"api/admin/searchitem?ItemName={CategoryName.ToLower()}&ItemType=category");
            //////////////////


            Assert.Equal(adminCredentials.Email.ToLower(), GetUserInfo.email);
            Assert.Equal(AuthorName.ToLower(), GetAuthorName.fullname);
            Assert.Equal(CategoryName.ToLower(), GetCategoryName.name);

        }









        [Fact]
        public async Task Insert_BookTest()
        {
            var AdminCredentials = new Login()
            {
                Email = "admin@example.com",
                Password = "password123"
            };

            string AuthorNameToinsert = "TestAuthorx";
            string CategoryNameToinsert = "TestCategoryx";

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






        [Fact]
        public async Task BookStock_Test()
        {

            var AdminCredentials = new Login()
            {
                Email = "admin@example.com",
                Password = "password123"
            };
            var Fakebook = new BookinsertModel
            {
                Title = "test",
                ISBN = "0000000000101",
                Price = 10,
                StockQuantity = 10,
                PublicationDate = new()
                {
                    year = 2000,
                    month = 1,
                    day = 1,
                },
                Description = "test",

            };

            await seed.BaseDatabookseed();
            await seed.InsertCustombook(Fakebook);
            await seed.InsertDummyuser(AdminCredentials, UserRole.admin);
            const int testAddint = 10;
            const int testOverrideInt = 2;


            /////////////////

            var adminTokenBody = await client.PostAsJsonAsync("auth/login", AdminCredentials);
            var adminTokenraw = adminTokenBody.Content.ReadFromJsonAsync<Tokenlogin>();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminTokenraw.Result.result);


            var additemstostockN = await client.PatchAsync($"api/bookstock/{Fakebook.ISBN}?qnty={testAddint}", null);

            int itemAddResult = await seed.Checkstockitemdb(Fakebook.ISBN);

            var ovverideitemstock = await client.PatchAsync($"api/bookstock/{Fakebook.ISBN}?qnty={testOverrideInt}&ForceOverride=True", new StringContent(string.Empty));

            int itemOverrideResult = await seed.Checkstockitemdb(Fakebook.ISBN);

            ////////////////

            Assert.Equal(Fakebook.StockQuantity + testAddint, itemAddResult);
            Assert.Equal(testOverrideInt, itemOverrideResult);

        }


        [Fact]
        public async Task BookPrice_Test()
        {

            var AdminCredentials = new Login()
            {
                Email = "admin@example.com",
                Password = "password123"
            };
            var Fakebook = new BookinsertModel
            {
                Title = "test",
                ISBN = "0000000000101",
                Price = 10,
                StockQuantity = 10,
                PublicationDate = new()
                {
                    year = 2000,
                    month = 1,
                    day = 1,
                },
                Description = "test",

            };

            await seed.BaseDatabookseed();
            await seed.InsertCustombook(Fakebook);
            await seed.InsertDummyuser(AdminCredentials, UserRole.admin);

            const decimal newPrice = 15.5M;

            ///////////////////

            var adminTokenBody = await client.PostAsJsonAsync("auth/login", AdminCredentials);
            var adminTokenraw = adminTokenBody.Content.ReadFromJsonAsync<Tokenlogin>();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminTokenraw.Result.result);

            var CurrentPrice = await seed.CheckBookPrice(Fakebook.ISBN);

            var ChangePrice = await client.PatchAsync($"api/bookprice/{Fakebook.ISBN}?price={newPrice.ToString(CultureInfo.InvariantCulture)}", null);

            var PriceUpdated = await seed.CheckBookPrice(Fakebook.ISBN);

            //////////////////


            Assert.Equal(Fakebook.Price, CurrentPrice);
            Assert.Equal(HttpStatusCode.OK, ChangePrice.StatusCode);
            Assert.Equal(newPrice, PriceUpdated);
            Assert.NotEqual(CurrentPrice, PriceUpdated);


        }


        [Fact]
        public async Task DeleteBook_ByISBN()
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
                Title = "testdeletebook",
                Author =  AuthorNameToinsert,
                Category = CategoryNameToinsert,
                ISBN = "0000000000111",
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



            await seed.InsertCustomAuthorCategory(AuthorNameToinsert, CategoryNameToinsert);
            await seed.InsertDummyuser(AdminCredentials, UserRole.admin);

            /////////////////////
            var adminTokenBody = await client.PostAsJsonAsync("auth/login", AdminCredentials);
            var adminTokenraw = adminTokenBody.Content.ReadFromJsonAsync<Tokenlogin>();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminTokenraw.Result.result);

            var insertBook = await client.PostAsJsonAsync("api/book", FakebookList);

            bool existAfterinsert = await seed.Checkbookexist(FakebookList[0].ISBN);

            var deletebook = await client.DeleteAsync($"/api/book/{FakebookList[0].ISBN}");

            var dexistAfterdelete = await seed.Checkbookexist(FakebookList[0].ISBN);

            ////////////////////

            Assert.Equal(HttpStatusCode.Created, insertBook.StatusCode);
            Assert.True(existAfterinsert);
            Assert.False(dexistAfterdelete);


        }

        [Fact]
        public async Task DeleteOthersAccount()
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var UserCredentials = new Login()
            {
                Email = "user@example.com",
                Password = "password123",
            };
            var AdminCredentials = new Login()
            {
                Email = "testadmin@example.com",
                Password = "admin"
            };

            await seed.InsertDummyuser(AdminCredentials, UserRole.admin);
            await seed.InsertDummyuser(UserCredentials);

            //////////////////////


            var UserLogin = await client.PostAsJsonAsync("auth/login", UserCredentials);

            var adminTokenBody = await client.PostAsJsonAsync("auth/login", AdminCredentials);
            var adminTokenraw = adminTokenBody.Content.ReadFromJsonAsync<Tokenlogin>();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminTokenraw.Result.result);
            var deleteuser = await client.DeleteAsync($"/api/account?email={UserCredentials.Email}");

            var UserTryTologin = await client.PostAsJsonAsync("auth/login", UserCredentials);
            var UserExist = await seed.UserExist(UserCredentials.Email);

            await seed.InsertDummyuser(UserCredentials);
            var UserTryTologinAfterInsert = await client.PostAsJsonAsync("auth/login", UserCredentials);
            var userTokenraw = await UserTryTologinAfterInsert.Content.ReadFromJsonAsync<Tokenlogin>();
            var userToken = tokenHandler.ReadJwtToken(userTokenraw.result);
            var userClaimUserID = userToken.Claims.First(x => x.Type == "UserID").Value;

            var deleteuserByuserID = await client.DeleteAsync($"/api/account?userid={userClaimUserID}");

            var existafterUserIDdel = await seed.UserExist(UserCredentials.Email);

            //////////////////////

            Assert.Equal(HttpStatusCode.OK, UserLogin.StatusCode);
            Assert.Equal(HttpStatusCode.Unauthorized, UserTryTologin.StatusCode);
            Assert.False(UserExist);
            Assert.Equal(HttpStatusCode.OK, UserTryTologinAfterInsert.StatusCode);
            Assert.Equal(HttpStatusCode.OK, deleteuserByuserID.StatusCode);
            Assert.False(existafterUserIDdel);



        }

        public async Task DisposeAsync()
        {
            await seed.CleanBooks();
            await seed.CleanUser();
        }

        public async Task InitializeAsync()
        {

            await Task.CompletedTask;
        }
    }
}
