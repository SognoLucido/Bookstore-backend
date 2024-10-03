using Auth;
using Auth._3rdpartyPaymentportal;
using Bookstore_backend.MigrationInit;
using Database.ApplicationDbcontext;
using Database.DatabaseLogic;
using Database.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text;


namespace Bookstore_backend
{
    public partial class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);



            builder.Services.AddControllers();

            //builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(opt =>
            {

                opt.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Bookstore-backend DEMO",
                    Description = "ASP.NET Core Web API",
                    Contact = new OpenApiContact
                    {
                        Name = "Francesco Barbano",
                        Url = new Uri("https://github.com/SognoLucido/Bookstore-backend")
                    }

                });


                opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Login using Bearer-token (/auth/login)",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Scheme = "Bearer"

                });




                opt.OperationFilter<AuthResponsesOperationFilter>();

                var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                opt.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));

            });
            builder.Services.AddScoped<IpassHash, Passhasher>();
            builder.Services.AddHttpClient<PaymentPortalx>();
            builder.Services.AddScoped<ICrudlayer, DbBookCrud>();
            builder.Services.AddSingleton<TokenBlocklist>();



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




            builder.Services.AddAuthorization(x =>
            {
                x.AddPolicy("UserOnly", p => p.RequireClaim("ruoli", "user"));
                x.AddPolicy("AdminOnly", p => p.RequireClaim("ruoli", "admin"));
            });




            builder.Services.AddDbContext<Booksdbcontext>(opt =>
                     opt.UseNpgsql(builder.Configuration.GetConnectionString("database")));


            builder.Services.AddScoped<ITokenService, JWTokenGenerator>();

            //builder.Services.AddDatabaseCrudService();




            var app = builder.Build();


            if (app.Environment.IsEnvironment("xunit"))
            {
                using var scope = app.Services.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<Booksdbcontext>();

                context.Database.EnsureCreated();
            }
            else
            {
                await app.ApplyMigration();

                app.UseSwagger();
                app.UseSwaggerUI(opt =>
                {
                    opt.SwaggerEndpoint("/swagger/v1/swagger.json", "BookStore_v1");
                    opt.RoutePrefix = string.Empty;
                });
            }

               

            


            //app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();



        }
    }

    public partial class Program { }

}
