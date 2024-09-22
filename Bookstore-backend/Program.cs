using Auth;
using Auth._3rdpartyPaymentportal;
using Bookstore_backend.MigrationInit;
using Bookstore_backend.MinimalAPIEndpoints;
using Bookstore_backend.Services;
using Database.ApplicationDbcontext;
using Database.DatabaseLogic;
using Database.Services;
using Microsoft.EntityFrameworkCore;



namespace Bookstore_backend
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);


            builder.Services.AddSwagger();
       

            builder.Services.AddScoped<IpassHash, Passhasher>();
            builder.Services.AddHttpClient<PaymentPortalx>();
            builder.Services.AddScoped<ICrudlayer, DbBookCrud>();
            builder.Services.AddSingleton<TokenBlocklist>();


            builder.Services.AddAuth(builder.Configuration);

            builder.Services.AddAuthorizationBuilder()
                .AddPolicy("UserOnly", p => p.RequireClaim("ruoli", "user"))
                .AddPolicy("AdminOnly", p => p.RequireClaim("ruoli", "admin"));




            builder.Services.AddDbContext<Booksdbcontext>(opt =>
                     opt.UseNpgsql(builder.Configuration.GetConnectionString("database")));


            builder.Services.AddScoped<ITokenService, JWTokenGenerator>();



            var app = builder.Build();

            app.ApplyMigration();

            //app.UseHttpsRedirection();
           
            app.UseSwagger();
            app.UseSwaggerUI(opt =>
            {
                opt.SwaggerEndpoint("/swagger/v1/swagger.json", "BookStore_v1");
                opt.RoutePrefix = string.Empty;
            });
           
            app.UseAuthentication();
            app.UseAuthorization();
     
            app.UseBookEndpoints();
            app.UseUserEndpoints();
            app.UseAuthEndpoints();
            app.UseAdminEndpoints();

            app.Run();
        }
    }
}
