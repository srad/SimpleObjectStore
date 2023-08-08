﻿using Microsoft.EntityFrameworkCore;
using SimpleObjectStore.Helpers;
using SimpleObjectStore.Models;
using SimpleObjectStore.Services;

namespace SimpleObjectStore.Seeds;

internal static class DbInitializer
{
    internal static async void Initialize(ApplicationDbContext dbContext, ApiKeyService service, StorageSlug storageSlug)
    {
        ArgumentNullException.ThrowIfNull(dbContext, nameof(dbContext));
        //await dbContext.Database.EnsureCreatedAsync();
        await dbContext.Database.MigrateAsync();
        await CreateApiKey(dbContext, service);
        await ImportFilesFromStorage(dbContext, storageSlug);
        await DeleteOrphans(dbContext);
        await CreateAllowedHosts(dbContext);
        await CreateAllowedHosts(dbContext);
    }

    private static async Task CreateAllowedHosts(ApplicationDbContext dbContext)
    {
        if (!await dbContext.AllowedHosts.AnyAsync())
        {
            await dbContext.AddAsync(new AllowedHost
            {
                Hostname = "*"
            });
            await dbContext.SaveChangesAsync();
        }
    }

    private static async Task DeleteOrphans(ApplicationDbContext dbContext)
    {
        var files = await dbContext.BucketFiles.ToListAsync();
        foreach (var file in files.Where(file => !File.Exists(file.FilePath)))
        {
            // If the file is not on disk, then also delete from database.
            await dbContext.BucketFiles
                .Where(x => x.StorageFileId == file.StorageFileId)
                .ExecuteDeleteAsync();
        }
    }

    private static async Task ImportFilesFromStorage(ApplicationDbContext dbContext, ISlug slugHelper)
    {
        var folder = Environment.GetEnvironmentVariable("STORAGE_DIRECTORY") ?? throw new MissingFieldException("storage path missing for initialization");
        var info = new DirectoryInfo(folder);

        foreach (var dir in info.GetDirectories())
        {
            // Import folders
            var bucket = await dbContext.Buckets.FirstOrDefaultAsync(x => x.DirectoryName == dir.Name);
            if (bucket == null)
            {
                var now = DateTimeOffset.Now;
                bucket = new Bucket
                {
                    BucketId = Guid.NewGuid().ToString(),
                    Name = dir.Name,
                    DirectoryName = dir.Name,
                    Private = false,
                    LastAccess = now,
                    CreatedAt = now,
                };
                await dbContext.Buckets.AddAsync(bucket);
                await dbContext.SaveChangesAsync();
            }

            // Import files
            var files = dir.GetFiles();
            foreach (var file in files)
            {
                if (await dbContext.BucketFiles.AnyAsync(x => x.StoredFileName == file.Name))
                {
                    continue;
                }

                var filename = slugHelper.Generate(file.Name);
                var newFile = new BucketFile()
                {
                    StorageFileId = Guid.NewGuid().ToString(),
                    FileName = file.Name,
                    StoredFileName = filename,
                    FilePath = Path.Combine(file.Directory.ToString(), filename),
                    CreatedAt = DateTimeOffset.Now,
                    FileSize = file.Length,
                    FileSizeMB = String.Format("{0:0.00}", (float)file.Length / 1024 / 1024),
                    Private = false,
                    AccessCount = 0,
                    Url = $"{bucket.DirectoryName}/{file.Name}",
                    BucketId = bucket.BucketId,
                    LastAccess = DateTimeOffset.Now
                };

                await dbContext.BucketFiles.AddAsync(newFile);
                await dbContext.SaveChangesAsync();
            }
        }
    }

    private static async Task CreateApiKey(ApplicationDbContext dbContext, ApiKeyService service)
    {
        if (await dbContext.ApiKeys.AnyAsync())
        {
            return;
        }

        var key = new ApiKey
        {
            Key = service.GenerateApiKey(), Title = "Default key", CreatedAt = DateTimeOffset.UtcNow, AccessTimeLimited = false
        };
        dbContext.ApiKeys.Add(key);

        Console.WriteLine($"The default key for the API is: {key.Key}");

        await dbContext.SaveChangesAsync();
    }
}