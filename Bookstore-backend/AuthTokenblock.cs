using Microsoft.AspNetCore.Authorization;

namespace Bookstore_backend
{
    public class AuthTokenblock : IAuthorizationHandler, IAuthorizationRequirement
    {
        public Task HandleAsync(AuthorizationHandlerContext context)
        {
            throw new NotImplementedException();
        }
    }
}
