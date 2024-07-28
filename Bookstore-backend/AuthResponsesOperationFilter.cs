using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Bookstore_backend
{
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
