using Auth._3rdpartyPaymentportal;
using Database.Model.ModelsDto;
using Database.Model.ModelsDto.Paymentmodels;
using Database.Model.ModelsDto.PaymentPartialmodels;
using Database.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using Database.Mapperdtotodb;
using Database.Model;

namespace Bookstore_backend.MinimalAPIEndpoints;

[Authorize]
public static class MinimalUserEndpoint
{
    
    public static async void UseUserEndpoints(this IEndpointRouteBuilder app)
    {
        var apiGroup = app.MapGroup("/api").WithTags("User").RequireAuthorization();

        
        apiGroup.MapGet("userinfo", GetAccountInfo)
            .Produces<UserInfo>(200);

        apiGroup.MapPost("buybook", UserBuyTransaction)
            .Produces<Invoicev2>(200);

        apiGroup.MapPost("buysubtier", BuySubscriptions);


        app.MapDelete("api/accountself", DeleteAccountSelf)
            .WithTags("User")
            .RequireAuthorization("UserOnly");





    }






    /// <summary>
    /// Get account information like -> apikey
    /// </summary>
    /// <returns></returns>                                      
    private static async Task<IResult> GetAccountInfo(HttpContext httpContext ,ICrudlayer dbcall,TokenBlocklist block)
    {

        var UserID = httpContext.User.Claims.SingleOrDefault(x => x.Type == "UserID");

        
        if (!Guid.TryParse(UserID.Value, out Guid userIDokcheck)) return Results.BadRequest();


        var dbdata = await dbcall.GetUserInfoAccount(userIDokcheck);


        return dbdata is not null ? Results.Ok(dbdata) : Results.NotFound("invalid UserID - user not found");


    }


    private static async Task<IResult> UserBuyTransaction(HttpContext httpContext, [FromBody] BookPartialPaymentModel data, ICrudlayer dbcall, PaymentPortalx portalpay)
    {


        var UserID = httpContext.User.Claims.SingleOrDefault(x => x.Type == "UserID");


        if (UserID is null) return Results.BadRequest();
        if (!Guid.TryParse(UserID.Value, out Guid GuidUserID)) return Results.BadRequest();
        if (httpContext.User.HasClaim("role", "admin")) return Results.BadRequest("only users");


        //if (!ModelState.IsValid)
        //{
        //    return BadRequest(ModelState);
        //}



        var dbdata = await dbcall.GetInvoicebooks(data.BookItemList, GuidUserID);
        if (dbdata.Item1 is null || dbdata.Item2 is null) return Results.BadRequest();



        dbdata.Item1.Fillcardinfo(data.PaymentDetails);


        var message = await portalpay.BookPaymentportal(dbdata.Item1, (int)dbdata.Item2, dbcall);





        return message.Code switch
        {
            //case 200: return Ok((dbdata.Item1.Invoce,dbdata.Item1.TotalAmount))
            200 => Results.Ok(new Invoicev2(dbdata.Item1.Invoce, dbdata.Item1.TotalAmount)),
            400 => Results.BadRequest(message.Message),
            _ => Results.StatusCode(500),
        };
        




    }

    //rip
    private record Invoicev2(List<Invoice> Invoice, decimal Total);





    /// <param name="subscriptionTier">
    /// - <c>Tier0</c>: Represents the free tier(default).
    /// - <c>Tier1</c>: Represents the standard subscription tier.
    /// - <c>Tier2</c>: Represents the premium subscription tier.
    /// </param>
    private static async Task<IResult> BuySubscriptions(HttpContext httpContext, [FromBody] PartialPaymentDetails data, ICrudlayer dbcall, [FromQuery] Subscription subscriptionTier, PaymentPortalx portalpay)
    {

        Guid UserdbGuid = Guid.Empty;
        UserRole? role = null;

        foreach (var claims in httpContext.User.Claims)
        {

            switch (claims.Type)
            {
                case "UserID":
                    {
                        if (claims.Value is null) return Results.BadRequest();
                        else UserdbGuid = Guid.Parse(claims.Value);
                    }; break;
                case "ruoli":
                    {
                        if (claims.Value is null) return Results.BadRequest();

                        if (claims.Value == "user") break;
                        else if (claims.Value == "admin") return Results.BadRequest("admin account doesn't need to buy a subscription");
                        else return Results.BadRequest();


                    };

            }

        }


        var backdata = await portalpay.Subpayment(UserdbGuid, subscriptionTier, dbcall);




        return backdata.Code == 200 ? Results.Ok() : Results.StatusCode(500);

    }




    private static async Task<IResult> DeleteAccountSelf(HttpContext httpContext, TokenBlocklist block, ICrudlayer dbcall)
    {
        var UserIDdata = httpContext.User.FindFirst("UserID").Value;
        if (!Guid.TryParse(UserIDdata, out Guid GuidUserID)) return Results.BadRequest();



        if (await dbcall.DeleteAccount(GuidUserID))
        {
            var rawtoken = httpContext.Request.Headers.Authorization.First();

            string EncodedSignature = rawtoken.Substring(rawtoken.Length - 43);

            block.TokenInsert(EncodedSignature);

            return Results.Ok("Deletion was successful");
        }
        else return Results.StatusCode(500);




    }







}







