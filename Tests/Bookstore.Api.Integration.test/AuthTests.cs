
using Bookstore.Api.Integration.test.Model;
using Database.Model.Apimodels;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;


namespace Bookstore.Api.Integration.test;

public class AuthTests : IClassFixture<ProgramTestApplicationFactory>
{

    private readonly ProgramTestApplicationFactory _factory;

    public AuthTests(ProgramTestApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Base_Authentication_Login()
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var _client = _factory.CreateClient();
        var TestUser = new Registration
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
        //This record (admin account) is automatically injected during database creation. The values are known; check MigrationInit in the main project
        var AdminCredentials = new Login()
        {
            Email = "admin@example.com",
            Password = "admin"
        };

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
        Assert.Equal("AdminAdmin", Admindata.firstname + Admindata.lastname);
        Assert.Equal(HttpStatusCode.OK, InsertUserBody.StatusCode);

    }

   



}