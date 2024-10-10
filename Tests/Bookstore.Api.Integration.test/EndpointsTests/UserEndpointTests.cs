using Bookstore.Api.Integration.test.Dataseed;
using Bookstore.Api.Integration.test.Model;
using Database.Model.Apimodels;
using Database.Model.ModelsDto;
using Database.Model.ModelsDto.Paymentmodels;
using Database.Model.ModelsDto.PaymentPartialmodels;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Bookstore.Api.Integration.test.EndpointsTests
{
    public class UserEndpointTests(ProgramTestApplicationFactory _factory) : IClassFixture<ProgramTestApplicationFactory>, IAsyncLifetime
    {

        private readonly ProgramTestApplicationFactory factory = _factory;
        private readonly DataseedperTestLogic seed = _factory.Services.GetRequiredService<DataseedperTestLogic>();



        [Fact]
        public async Task User_BuysubTier()
        {
            var client = factory.CreateClient();

            var UserCredentials = new Login()
            {
                Email = "user@example.com",
                Password = "password123"
            };

            await seed.InsertDummyuser(UserCredentials);

            var paymentdata = new PartialPaymentDetails
            {
                CardHolderName = "test",
                CardNumber = "1234567890123456",
                CardCVC = "123",
                CardExpiry = new()
                {
                    Year = 50,
                    Month = 1
                }
            };


            ////////////////

            var UserTokenBody = await client.PostAsJsonAsync("auth/login", UserCredentials);
            var UserTokenraw = UserTokenBody.Content.ReadFromJsonAsync<Tokenlogin>();

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", UserTokenraw.Result.result);
            var buytier1 = await client.PostAsJsonAsync("api/buysubtier?subscriptionTier=1", paymentdata);
            var tier1check = await client.GetFromJsonAsync<SubtierInfo>("/api/userinfo");

            var buytier2 = await client.PostAsJsonAsync("api/buysubtier?subscriptionTier=2", paymentdata);
            var tier2check = await client.GetFromJsonAsync<SubtierInfo>("/api/userinfo");


            /////////////////

            Assert.Equal("Tier1", tier1check.apiInfo.subscriptionTier);
            Assert.Equal("Tier2", tier2check.apiInfo.subscriptionTier);




        }

        [Fact]
        public async Task User_buybook()
        {
            var client = factory.CreateClient();

            await seed.BaseDatabookseed();

            var UserCredentials = new Login()
            {
                Email = "user@example.com",
                Password = "password123"
            };

            await seed.InsertDummyuser(UserCredentials);

            var fakeBook = new BookinsertModel
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

            await seed.InsertCustombook(fakeBook);

            var postRequest = new BookPartialPaymentModel
            {
                PaymentDetails = new PartialPaymentDetails
                {
                    CardHolderName = "test",
                    CardNumber = "1234567890123456",
                    CardCVC = "123",
                    CardExpiry = new()
                    {
                        Year = 50,
                        Month = 1
                    }
                },

                BookItemList =
               [
                   new(){ ISBN = fakeBook.ISBN , Quantity = fakeBook.StockQuantity+1}  // overstock test

               ]

            };


            //////////////////////



            var UserTokenBody = await client.PostAsJsonAsync("auth/login", UserCredentials);
            var UserTokenraw = UserTokenBody.Content.ReadFromJsonAsync<Tokenlogin>();

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", UserTokenraw.Result.result);
            var buyingOverstock = await client.PostAsJsonAsync("api/buybook", postRequest);


            postRequest.BookItemList = [new() { ISBN = fakeBook.ISBN, Quantity = fakeBook.StockQuantity }];

            var buybook = await client.PostAsJsonAsync("api/buybook", postRequest);




            ///////////////////

            Assert.Equal(HttpStatusCode.BadRequest, buyingOverstock.StatusCode);  //The request is dropped before reaching the payment portal (check it in db and drop)
            Assert.Equal(HttpStatusCode.OK, buybook.StatusCode);




        }

        [Fact]
        public async Task Delete_account()
        {
            var client = factory.CreateClient();

            var UserCredentials = new Login()
            {
                Email = "user@example.com",
                Password = "password123"
            };

            await seed.InsertDummyuser(UserCredentials);

            //////////////////

            var UserTokenBody = await client.PostAsJsonAsync("auth/login", UserCredentials);
            var UserTokenraw = UserTokenBody.Content.ReadFromJsonAsync<Tokenlogin>();

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", UserTokenraw.Result.result);
            var checkUserexistPart1 = await client.GetAsync("api/userinfo");

            var deleteAccount = await client.DeleteAsync("api/accountself");
            var checkUserexistPart2 = await client.GetAsync("api/userinfo");

            /////////////////

            Assert.Equal(HttpStatusCode.OK, checkUserexistPart1.StatusCode);
            Assert.Equal(HttpStatusCode.OK, deleteAccount.StatusCode);
            Assert.Equal(HttpStatusCode.Unauthorized, checkUserexistPart2.StatusCode);



        }





        public Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        public async Task DisposeAsync()
        {
            await seed.CleanUser();
        }



    }
}
