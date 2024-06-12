using Database.ApplicationDbcontext;
using Microsoft.EntityFrameworkCore;

namespace Bookstore_backend
{
    public static class MigrationInitialization
    {
        public static void ApplayMigration(this IApplicationBuilder app)
        {
            using IServiceScope scope = app.ApplicationServices.CreateScope();

            using Booksdbcontext dbContext = scope.ServiceProvider.GetRequiredService<Booksdbcontext>();

            dbContext.Database.Migrate();

        }
    }
}
