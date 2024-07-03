using Auth;
using Auth._3rdpartyPaymentportal;
using Database;
using Database.ApplicationDbcontext;
using Database.DatabaseLogic;
using Database.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;

using Microsoft.IdentityModel.Tokens;
using System.Text;


namespace Bookstore_backend
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);



            builder.Services.AddControllers();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddScoped<IpassHash, Passhasher>();
            builder.Services.AddHttpClient<PaymentPortalx>();


            builder.Services.AddAuthentication(
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
                    ValidIssuer = builder.Configuration["Authentication:Issuer"],
                    ValidAudience = builder.Configuration["Authentication:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Authentication:SecretKey"]!))
                };
            });



            builder.Services.AddAuthorization(x =>
            {
                x.AddPolicy("Userlogged", p => p.RequireClaim("ruoli", "user" ,"admin"));
                x.AddPolicy("AdminOnly", p => p.RequireClaim("ruoli", "admin"));
            });

               
               

            builder.Services.AddDbContext<Booksdbcontext>(opt =>
                     opt.UseNpgsql(builder.Configuration.GetConnectionString("database")));


            builder.Services.AddScoped<ITokenService, JWTokenGenerator>();

            builder.Services.AddDatabaseCrudService();


            var app = builder.Build();


            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.ApplayMigration();


            app.UseHttpsRedirection();


            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
