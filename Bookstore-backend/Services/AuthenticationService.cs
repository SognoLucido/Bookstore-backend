using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Bookstore_backend.Services
{
    public static  class AuthenticationService
    {


        public static void AddAuth(this IServiceCollection Services,IConfiguration configuration)
        {
            Services.AddAuthentication(
               opt =>
               {
                   opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                   opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                   opt.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;

               }).AddJwtBearer(opt =>
               {
                   opt.TokenValidationParameters = new()
                   {
                       ValidateIssuer = true,
                       ValidateAudience = true,
                       ValidateIssuerSigningKey = true,
                       ValidateLifetime = true,
                       ValidIssuer = configuration["Authentication:Issuer"],
                       ValidAudience = configuration["Authentication:Audience"],
                       IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Authentication:SecretKey"]!))
                   };





                   opt.Events = new JwtBearerEvents
                   {
                       OnTokenValidated = context =>
                       {

                           var tokenBlocklist = context.HttpContext.RequestServices.GetRequiredService<TokenBlocklist>();
                           var token = ((Microsoft.IdentityModel.JsonWebTokens.JsonWebToken)context.SecurityToken).EncodedSignature;

                           //Console.WriteLine($"check token : {token}");

                           if (tokenBlocklist.TokenListCheck(token))
                           {
                               context.Fail("Token is blacklisted.");
                           }

                           return Task.CompletedTask;
                       }
                   };


               });
        }

    }
}
