using System.Net;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using SimpleObjectStore.Models;

namespace SimpleObjectStore.Http;

public static class StaticFileOptionsHandler
{
    public static async void OnPrepareResponse(WebApplication app, StaticFileResponseContext ctx)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        if (!await db.AllowedHosts.AnyAsync(x => x.Hostname == "*" || x.Hostname == ctx.Context.Request.Host.ToString()))
        {
            ctx.Context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            ctx.Context.Response.ContentLength = 0;
            ctx.Context.Response.Body = Stream.Null;
            ctx.Context.Response.Headers.Add("Cache-Control", "no-store");
            return;
        }

        var dir = new DirectoryInfo(ctx.File.PhysicalPath!);
        var filename = dir.Name;
        var bucketId = await db.BucketFiles.Where(x => x.FilePath == dir.ToString())
            .Select(x => x.BucketId)
            .FirstAsync();

        var timestamp = DateTimeOffset.Now;

        // Bucket private?
        var bucketProhibited = await db.Buckets.AnyAsync(x => x.BucketId == bucketId && x.Private && !ctx.Context.User.Identity.IsAuthenticated);

        // File private?
        var fileProhibited = await db.BucketFiles.AnyAsync(x => x.StoredFileName == filename && x.BucketId == bucketId && x.Private && !ctx.Context.User.Identity.IsAuthenticated);
        
        if (bucketProhibited || fileProhibited)
        {
            ctx.Context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            ctx.Context.Response.ContentLength = 0;
            ctx.Context.Response.Body = Stream.Null;
            ctx.Context.Response.Headers.Add("Cache-Control", "no-store");
        }

        // Update counter and timestamp.
        await db.Buckets.Where(x => x.BucketId == bucketId)
            .ExecuteUpdateAsync(x => x.SetProperty(p => p.LastAccess, timestamp));

        await db.BucketFiles.Where(x => x.StoredFileName == filename && x.BucketId == bucketId)
            .ExecuteUpdateAsync(x => x.SetProperty(p => p.AccessCount, p => p.AccessCount + 1)
                .SetProperty(p => p.LastAccess, p => timestamp));

        // Could add an option to the database that the file is "download only".
        //ctx.Context.Response.Headers["Content-Disposition"] = $"attachment; filename={file.FileName}";
    }
}