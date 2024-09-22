
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace Bookstore_backend.Services;

public static class SwaggerService
{


    public static void AddSwagger(this IServiceCollection Services)
    {
        Services.AddEndpointsApiExplorer();
        
        Services.AddSwaggerGen(opt =>
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

    }



    public class AuthResponsesOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var authAttributes = context.MethodInfo.DeclaringType.GetCustomAttributes(true)
                .Union(context.MethodInfo.GetCustomAttributes(true))
                .OfType<AuthorizeAttribute>();


            if (authAttributes.Any())
            {
                operation.Security.Add(new OpenApiSecurityRequirement
                 {
                     {
                         new OpenApiSecurityScheme
                         {
                             Reference = new OpenApiReference
                             {
                                 Type = ReferenceType.SecurityScheme,
                                 Id = "Bearer"
                             }
                         }, Array.Empty<string>()
                     }
                 });

                //operation.Responses.Add("401", new OpenApiResponse { Description = "Unauthorized" });
            }

        }
    }




}
