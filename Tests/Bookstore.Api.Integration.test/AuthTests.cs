using Bookstore.Api.Integration.test.Dataseed;
using Bookstore.Api.Integration.test.Model;
using Database.DatabaseLogic;
using Database.Model;
using Database.Model.Apimodels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;


namespace Bookstore.Api.Integration.test;

public class AuthTests(ProgramTestApplicationFactory factory) : IClassFixture<ProgramTestApplicationFactory> 
{

    private readonly ProgramTestApplicationFactory _factory = factory;
  

    [Fact]
    public async Task Base_Authentication_Login()
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var _client = _factory.CreateClient();
        var DBseed = _factory.Services.GetService<DataseedperTestLogic>();

        var TestUser = new Registration //safetoedit
        {
            FirstName = "test",
            LastName = "user",
            Email = "user@example.com",
            Password = "password123",
            Address = "string",
            Phone = "string"

        };
        var UserCredentials = new Login()
        {
            Email = TestUser.Email,
            Password = TestUser.Password,
        };  
        var AdminCredentials = new Login()  //safetoedit
        {
            Email = "testadmin@example.com",
            Password = "admin"
        };
        var AdminCreation = new Customer()
        {
            Id = Guid.NewGuid(),
            FirstName = "Admin",
            LastName = "Admin",
            Email = AdminCredentials.Email,
            //password
            //salt
            Address = "home",
            Phone = "string",
            RolesModelId = (int)UserRole.admin

        };

        
        await DBseed.AuthDataSeedInit(AdminCreation, AdminCredentials.Password);

     
        ////////////////////////

        //token roles validation logic
        var InsertUserBody = await _client.PostAsJsonAsync("auth/register", TestUser);
        var UserTokenBody = await _client.PostAsJsonAsync("auth/login",UserCredentials );
        var AdminTokenBody = await _client.PostAsJsonAsync("auth/login", AdminCredentials);

        var UserTokenraw = UserTokenBody.Content.ReadFromJsonAsync<Tokenlogin>();
        var AdminTokenraw = AdminTokenBody.Content.ReadFromJsonAsync<Tokenlogin>();

        var UserToken =  tokenHandler.ReadJwtToken(UserTokenraw.Result.result);
        var AdminToken = tokenHandler.ReadJwtToken(AdminTokenraw.Result.result);

        var UserClaimRole = UserToken.Claims.SingleOrDefault(x => x.Type == "ruoli");
        var AdminClaimRole = AdminToken.Claims.SingleOrDefault(x => x.Type == "ruoli");

        //user data validation logic

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", UserTokenraw.Result.result);
        
        var Userdata = await _client.GetFromJsonAsync<Data>("api/userinfo");

     
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AdminTokenraw.Result.result);
        var Admindata = await _client.GetFromJsonAsync<Data>("api/userinfo");

        ////////////////////////

        Assert.Equal("user", UserClaimRole.Value);
        Assert.Equal("admin", AdminClaimRole.Value);
        Assert.Equal(TestUser.FirstName + TestUser.LastName, Userdata.firstname + Userdata.lastname);
        Assert.Equal(AdminCreation.FirstName+ AdminCreation.LastName, Admindata.firstname + Admindata.lastname);
        Assert.Equal(TestUser.Email, Userdata.email);
        Assert.Equal(AdminCredentials.Email, Admindata.email);
        Assert.Equal(HttpStatusCode.OK, InsertUserBody.StatusCode);

    }

 
}