using Database.ApplicationDbcontext;
using Database.DatabaseLogic;
using Database;
using Microsoft.EntityFrameworkCore;
using Database.Services;
using System.Data;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Auth;


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

            builder.Services.AddAuthentication("Bearer").AddJwtBearer(opt =>
            {
                opt.TokenValidationParameters = new()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["Authentication:Issuer"],
                    ValidAudience = builder.Configuration["Authentication:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(builder.Configuration["Authentication:SecretKey"]!))
                };
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

            app.UseAuthorization();
            app.UseAuthentication();

            app.MapControllers();

            app.Run();
        }
    }
}
