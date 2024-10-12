using Database.Model.Apimodels;
using Database.Services;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using static System.Net.Mime.MediaTypeNames;

namespace Bookstore_backend.MinimalAPIEndpoints
{

    public static class MinimalBookEndpoint
    {


        public static async void UseBookEndpoints(this IEndpointRouteBuilder app)
        {

            var apiGroup = app.MapGroup("/api").WithTags("BookStore");



            apiGroup.MapGet("search", SearchItems)
                .Produces<DetailedFilterBookModel>(200)
                .Produces(404);

            apiGroup.MapGet("booklist", GetBookList)
                 .Produces<List<BooksCatalog>>(200)
                 .Produces(404);

            app.MapGet("apikey/{ISBN}", ApiService)
                .WithTags("Bookstore")
                .Produces<MarketDataAPIModelbyISBN>(200);
                

           
        }




        /// <summary>
        /// substring matching with limit result hardcoded to 5 
        /// </summary>
        /// <param name="booktitle">optional</param>
        /// <param name="authorname">optional</param>
        /// <param name="category">optional</param>   
        /// <returns></returns>
        private static async Task<IResult> SearchItems(string? booktitle,string? authorname,string? category,ICrudlayer dbcall,CancellationToken cToken)
        {

                const int limit = 5;

                var datain = (booktitle, authorname, category);
                var resposte = await dbcall.SearchItems(limit, datain, cToken);


                return resposte.Count > 0  ? Results.Ok(resposte) : Results.NotFound();

        }



        private static async Task<IResult> GetBookList([Range(1,int.MaxValue)]int page ,[Range(1,50)]int pagesize, ICrudlayer dbcall, CancellationToken cToken)
        {
  

            var data = await dbcall.RawReturn(page, pagesize, cToken);

            return data.Count > 0 ? Results.Ok(data) : Results.NotFound();

        }

        /// <summary>
        ///Simulates a premium service. It requires the x-api-key header for authentication. Check the /api/userinfo endpoint (login required) for the API key
        /// </summary>   
        /// <param name="apikey">The API key used to authenticate the request.</param>   
        /// <remarks>
        /// Sample request:
        /// 
        ///     GET /
        ///     
        ///    curl -X 'GET' /  
        ///    'https://localhost:7178/api/0000000000001'
        ///    -H 'accept: application/json' 
        ///    -H 'x-api-key: 000000000000000000004ae86ca65fc2'
        ///    
        /// </remarks>
        private static async Task<IResult> ApiService([RegularExpression("^[0-9]{13}$")]string ISBN, ICrudlayer dbcall, [FromHeader(Name = "x-api-key")] string apikey, CancellationToken cToken)
        {


            if (!Guid.TryParse(apikey, out var key)) return Results.BadRequest("invalid key");


            var (data, Errors) = await dbcall.ApiServiceGetbyisbn(ISBN, key, cToken);


            return data is not null ? Results.Ok(data) : Results.Json(Errors, statusCode : Errors!.Code);

        }



    }




}

