using Microsoft.EntityFrameworkCore;
using SimpleObjectStore.Models;

namespace SimpleObjectStore.Seeds;

internal static class DbInitializerExtension
{
    public static IApplicationBuilder UseDbInit(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app, nameof(app));

        using var scope = app.ApplicationServices.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        if (context != null)
        {
            if (context.Database.GetPendingMigrations().Any())
            {
                context.Database.Migrate();
            }
        }

        return app;
    }
}