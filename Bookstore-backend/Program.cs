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
    public partial class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);


            builder.Services.AddSwagger();
       

            //builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(opt =>
            {

                opt.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Bookstore-backend DEMO",
            builder.Services.AddAuthorizationBuilder()
                .AddPolicy("UserOnly", p => p.RequireClaim("ruoli", "user"))
                .AddPolicy("AdminOnly", p => p.RequireClaim("ruoli", "admin"));




                });


                opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,

                    Name = "Authorization",
                    BearerFormat = "JWT",
                    Scheme = "Bearer"

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


            builder.Services.AddAuthentication(
     
            app.UseBookEndpoints();
            app.UseUserEndpoints();
            app.UseAuthEndpoints();
            app.UseAdminEndpoints();

            app.Run();


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
                //x.AddPolicy("test",
                //    x => x.AddRequirements(new AuthTokenblock()));

                ////x.AddPolicy( new AuthTokenblock());

                x.AddPolicy("UserOnly", p => p.RequireClaim("ruoli", "user"));
                x.AddPolicy("AdminOnly", p => p.RequireClaim("ruoli", "admin"));
            });




            builder.Services.AddDbContext<Booksdbcontext>(opt =>
                     opt.UseNpgsql(builder.Configuration.GetConnectionString("database")));


            builder.Services.AddScoped<ITokenService, JWTokenGenerator>();

            //builder.Services.AddDatabaseCrudService();




            var app = builder.Build();

          

            app.ApplyMigration();

            //app.UseHttpsRedirection();
            //if (app.Environment.IsDevelopment())
            //{
            app.UseSwagger();
            app.UseSwaggerUI(opt =>
            {
                opt.SwaggerEndpoint("/swagger/v1/swagger.json", "BookStore_v1");
                opt.RoutePrefix = string.Empty;
            });
            //}




           


            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

        }
    }

    public partial class Program { }

}
