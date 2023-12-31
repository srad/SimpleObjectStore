﻿using SimpleObjectStore.Helpers.Interfaces;
using SimpleObjectStore.Models;
using SimpleObjectStore.Services;
using SimpleObjectStore.Services.Interfaces;

namespace SimpleObjectStore.Seeds;

internal static class DbInitializerExtension
{
    public static IApplicationBuilder UseDbInit(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app, nameof(app));

        using var scope = app.ApplicationServices.CreateScope();
        var services = scope.ServiceProvider;
        try
        {
            var context = services.GetRequiredService<ApplicationDbContext>();
            var service = services.GetRequiredService<IKeyService>();
            var slug = services.GetRequiredService<ISlug>();
            DbInitializer.Initialize(context, service, slug);
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error occurred creating the DB.");
        }

        return app;
    }
}