using Microsoft.AspNetCore.Authorization;

namespace Bookstore_backend
{
    public class AuthTokenblock : IAuthorizationHandler, IAuthorizationRequirement
    {
        public Task HandleAsync(AuthorizationHandlerContext context)
        {

            if (context.User.HasClaim("ruoli", "user" )){


                var tokenv1 = context.User.FindFirst(c => c.Type == "access_token")?.Value;

                var token = context.Requirements.FirstOrDefault();

                var x = context.User.Identity;


                
              
                context.Succeed(this);
                return Task.CompletedTask;
            }

            return Task.CompletedTask;

           // throw new NotImplementedException();
        }

        
    }
}
