using Database.Model;
using Database.Model.Apimodels;
using Database.Model.ModelsDto;
using Database.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;

namespace Bookstore_backend.MinimalAPIEndpoints;


[Authorize]
public static class MinimalAdminEndpoint
{


    public static async void UseAdminEndpoints(this IEndpointRouteBuilder app)
    {
        var apiGroup = app.MapGroup("/api").WithTags("Admin").RequireAuthorization("AdminOnly");


        apiGroup.MapGet("admin/search", SearchItems)
           .Produces<List<DetailedFilterBookModel>>(200);

        apiGroup.MapPost("changerole", Changerole);

        apiGroup.MapPost("book", InsertBook)
        .Produces<List<DetailedFilterBookModel>>(200)
        .Produces<Respostebookapi>(400)
        .Produces<Respostebookapi>(500);

        apiGroup.MapPost("upsertINFO", InsertCategoryxAuthor);

        apiGroup.MapPatch("bookstock/{ISBN}", AddOrOverrideStockQuantitybyISBN);

        apiGroup.MapPatch("bookprice/{ISBN}", Changeprice);

        apiGroup.MapDelete("book/{ISBN}", DeletebyISBN);

        apiGroup.MapDelete("account", DeleteAccount);

    }



    /// <summary>
    /// substring matching no limit 
    /// </summary>
    /// <param name="booktitle">optional</param>
    /// <param name="authorname">opt</param>
    /// <param name="category">opt</param> 
    /// <param name="limit">opt</param> 
    /// <returns></returns>
    private static async Task<IResult> SearchItems(
         string? booktitle,
         string? authorname,
         string? category,
         int? limit,
         ICrudlayer dbcall,
        CancellationToken cToken
        )
    {


        var templatedata = (booktitle, authorname, category);


        var data = await dbcall.SearchItems(limit, templatedata, cToken);

        return Results.Ok(data);
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="data">
    /// "role" : "admin" or "user" <br />
    /// email or userID
    /// </param>
    /// <remarks>
    /// Sample request:
    /// 
    ///     POST /
    ///     {
    ///         "email" : "user2@example.com"
    ///         "role" : "admin"
    ///     }
    ///    
    /// </remarks>
    /// <returns></returns>
    private static async Task<IResult> Changerole([FromBody] Rolechanger data, ICrudlayer dbcall)
    {

        if (data.UserID is null && data.email is null) return Results.BadRequest("At least one userid or email, is required");


        UserRole role;

        switch (data.Role.ToLower())
        {
            case "admin": role = UserRole.admin; break;
            case "user":
                {
                    if (data.UserID == Guid.Parse("8233a0ab-78ac-4ee7-916f-0cbb93e85a63") || data.email == "admin@example.com") return Results.BadRequest();

                    role = UserRole.user;
                }
                break;

            default: return Results.BadRequest("Invalid role.  choose one: admin , user");
        }


        if (await dbcall.ChangeRoles(data.UserID, data.email, role)) return Results.Ok();

        return Results.NotFound();

    }


    /// <remarks>
    /// Sample request:
    /// 
    ///    Author name and category MUST match the existing records
    ///    
    /// </remarks>
    private static async Task<IResult> InsertBook([FromBody][Required] List<BookinsertModel?> bodydata, ICrudlayer dbcall)
    {

        if (bodydata.Count == 0) return Results.BadRequest();

        foreach (var item in bodydata)
        {
            if (item.PublicationDate.year > DateTime.UtcNow.Year) return Results.BadRequest("wrong year");
        }

       

        //return

        var (booklist, ErrStatuscode) = await dbcall.InsertBooksItem(bodydata);


        return booklist is null ? Results.Json( ErrStatuscode , statusCode : ErrStatuscode?.Code ?? 500) : Results.Json(booklist,statusCode:201);
    }


    /// <summary>
    /// Upsert a list of authors or a list of categories in the database
    /// </summary>
    /// <param name="AuthorUpinsert">
    /// - <c>True</c>: Content in the Author that matches existing records in the database will be updated(bio) as provided, and non-existent records will be added .
    /// - <c>False/default(--)</c>: Add only /the non-existent record/s ,
    /// </param>
    /// <returns></returns>
    private static async Task<IResult> InsertCategoryxAuthor([FromBody] CategoryandAuthorDto body, ICrudlayer dbcall,[FromQuery] bool AuthorUpinsert = false)
    {


        if (body.Category is null && body.Author is null) return Results.BadRequest();

        return await dbcall.UpinsertAuthorsxCategories(body, AuthorUpinsert) == true ? Results.Ok() : Results.BadRequest();


    }


    /// <param name="ForceOverride">If true, the qnty query will override the current stock in the database; otherwise, Dbstocktotal will be incremented by qnty</param>
    /// <returns></returns>
    public static async Task<IResult> AddOrOverrideStockQuantitybyISBN(
           [FromRoute][RegularExpression("^[0-9]{13}$")] string ISBN,
            int qnty,
            bool? ForceOverride, // if true override the current dbstock-qnty with the qnty  , if not just add += qnty to the dbstock qnty 
           ICrudlayer dbcall,
            CancellationToken cToken)
    {
        if (await dbcall.AddOrOverrideStockQuantitybyISBN(ISBN, qnty, ForceOverride ?? false, cToken))
        {
            return Results.Ok();
        }
        return Results.NotFound();
    }



    public static async Task<IResult> Changeprice(
        [FromRoute][RegularExpression("^[0-9]{13}$")] string ISBN,
        decimal price,
        ICrudlayer dbcall,
        CancellationToken ctoken)
    {

        if (await dbcall.Pricebookset(ISBN, price, ctoken)) return Results.Ok();

        return Results.Problem("update failed",statusCode: 500 );
    }


    public static async Task<IResult> DeletebyISBN(
        [FromRoute][RegularExpression("^[0-9]{13}$")] string ISBN,
        ICrudlayer dbcall,
        CancellationToken ctoken)
    {
        if (await dbcall.Deletebyisbn(ISBN, ctoken))
        {
            return Results.Ok();
        }
        return Results.NotFound();
    }

    /// <summary>
    /// Delete a user account by User ID or email. (Admins can delete user accounts but cannot delete other admin accounts.)
    /// </summary>
    /// <returns></returns>
    public static async Task<IResult> DeleteAccount(Guid? userid, [EmailAddress] string? email,ICrudlayer dbcall, HttpContext httpC)
    {

        if (userid is null && email is null) return Results.BadRequest();
        if (!httpC.User.HasClaim("ruoli", "admin")) return Results.BadRequest();

     


        if (userid is not null)
        {
            if (await dbcall.DeleteAccount(userid)) return Results.Ok("Deletion was successful");
            else return Results.NotFound();
        }
        else if (email is not null)
        {
            if (await dbcall.DeleteAccount(email)) return Results.Ok("Deletion was successful");
            else return Results.NotFound();
        }


        return Results.StatusCode(500);


    }




}
