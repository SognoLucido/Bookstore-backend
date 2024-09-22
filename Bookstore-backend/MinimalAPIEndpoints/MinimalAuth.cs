using Auth;
using Database.Model.Apimodels;
using Database.Services;
using Microsoft.AspNetCore.Mvc;

namespace Bookstore_backend.MinimalAPIEndpoints;

public static class MinimalAuthEndpoint
{

    public static async void UseAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var apiGroup = app.MapGroup("/auth").WithTags("Auth");

        apiGroup.MapPost("register", Register);
        apiGroup.MapPost("login", Login);
    }




    private static async Task<IResult> Register([FromBody] Registration regi, ICrudlayer dbcall, CancellationToken token)
    {
        


        if (await dbcall.Registration(regi, token))
        {
            return Results.Ok();
        }
        else return Results.BadRequest("User already exist");

    }


    /// <summary>
    /// adminAccount :  email -> admin@example.com , passw -> admin
    /// </summary>
    private static async Task<IResult> Login([FromBody] Login login, ICrudlayer dbcall,  ITokenService tokengen, CancellationToken ctoken)
    {

        var data = await dbcall.Login(login, ctoken);

        if (data.Item1 is not null && data.Item2 is not null)
        {
            return Results.Ok(tokengen.GenerateToken(data.Item1, data.Item2));
        }
        else
        {
            return Results.Unauthorized();
        }
    }


}
