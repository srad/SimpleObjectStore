using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimpleObjectStore.Helpers.Interfaces;
using SimpleObjectStore.Models;
using SimpleObjectStore.Models.DTO;
using SimpleObjectStore.Services.Interfaces;

namespace SimpleObjectStore.Services;

public class StorageService(ApplicationDbContext context, ISlug slug, ILogger<StorageService> logger) : IStorageService
{
    private readonly string _storagePath = Environment.GetEnvironmentVariable("STORAGE_DIRECTORY") ?? throw new ArgumentNullException();
    private string BucketPath(string directoryName) => Path.Combine(_storagePath, directoryName);

    public async Task<IEnumerable<BucketFile>> ToListAsync() => await context.BucketFiles.AsNoTracking().ToListAsync();

    public async Task<ActionResult<BucketFile>> FindByIdAsync(string id)
    {
        var storageFile = await context.BucketFiles.FindAsync(id);

        if (storageFile == null)
        {
            throw new Exception("Storage item not found");
        }

        return storageFile;
    }

    /// <summary>
    /// Notice that this function slugs the filename before comparing it.
    /// It does no raw comparison of names.
    /// </summary>
    /// <param name="bucketId"></param>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public async Task<bool> ExistsAsync(string bucketId, string fileName) => await context.BucketFiles.AnyAsync(x => x.BucketId == bucketId && x.StoredFileName == slug.Generate(fileName));

    public async Task<List<CreateStorageFileResult>> SaveAsync(string bucketId, List<IFormFile> files)
    {
        var bucket = await context.Buckets
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.BucketId == bucketId);

        if (bucket == null)
        {
            throw new Exception("Bucket not found");
        }

        var results = new List<CreateStorageFileResult>();
        foreach (var file in files)
        {
            try
            {
                // Either a random filename is produced or the original converted into a slug.

                //var ext = Path.GetExtension(file.FileName);
                //fileNameSlug = $"{Guid.NewGuid().ToString()}{ext}";
                // Create slug and check if already exists.
                var fileNameSlug = slug.Generate(file.FileName);
                var fileExists = await context.BucketFiles.AnyAsync(x => x.BucketId == bucketId && x.StoredFileName == fileNameSlug);
                if (fileExists)
                {
                    results.Add(new CreateStorageFileResult
                    {
                        FileName = file.FileName,
                        StorageFile = await context.BucketFiles.FirstAsync(x => x.BucketId == bucketId && x.StoredFileName == fileNameSlug),
                        Success = false,
                        ErrorMessage = $"A file '{fileNameSlug}' already exists in this bucket"
                    });
                    continue;
                }

                // Upload
                var filePath = Path.Combine(BucketPath(bucket.DirectoryName), fileNameSlug);
                await using var stream = System.IO.File.OpenWrite(filePath);
                await file.CopyToAsync(stream);

                var storage = new BucketFile
                {
                    StorageFileId = Guid.NewGuid().ToString(),
                    FileName = file.FileName,
                    StoredFileName = fileNameSlug,
                    FilePath = filePath,
                    CreatedAt = DateTimeOffset.Now,
                    FileSize = stream.Length,
                    FileSizeMB = String.Format("{0:0.00}", (float)stream.Length / 1024 / 1024),
                    Private = false,
                    AccessCount = 0,
                    Url = $"{bucket.DirectoryName}/{fileNameSlug}",
                    BucketId = bucketId,
                    LastAccess = DateTimeOffset.Now,
                };
                await context.BucketFiles.AddAsync(storage);
                await context.SaveChangesAsync();
                results.Add(new CreateStorageFileResult()
                {
                    FileName = file.FileName,
                    StorageFile = storage,
                    Success = true,
                });
            }
            catch (Exception e)
            {
                logger.LogError(e.Message);
                results.Add(new CreateStorageFileResult()
                {
                    FileName = file.FileName,
                    ErrorMessage = e.Message,
                    Success = false
                });
            }
        }

        return results;
    }

    public async Task DeleteAsync(string id)
    {
        var storageFile = await context.BucketFiles.FindAsync(id);
        if (storageFile == null)
        {
            throw new Exception("File not found");
        }

        try
        {
            logger.LogInformation("Deleting file '{FilePath}'", storageFile.FilePath);
            File.Delete(storageFile.FilePath);
            context.BucketFiles.Remove(storageFile);
            await context.SaveChangesAsync();
        }
        catch (Exception e)
        {
            logger.LogError(e.Message);
            throw new Exception($"Error deleting '{storageFile.FilePath}': {e.Message}");
        }
    }

    public async Task PrivateAsync(string id) => await context.BucketFiles
        .Where(x => x.StorageFileId == id)
        .ExecuteUpdateAsync(x => x.SetProperty(p => p.Private, true));

    public async Task PublicAsync(string id) => await context.BucketFiles
        .Where(x => x.StorageFileId == id)
        .ExecuteUpdateAsync(x => x.SetProperty(p => p.Private, false));

    public StorageStats GetStorageStatsAsync()
    {
        var drive = new DriveInfo(_storagePath);
        var free = (float)drive.TotalFreeSpace / 1024 / 1024 / 1024;
        var total = (float)drive.TotalSize / 1024 / 1024 / 1024;

        return new StorageStats
        {
            FreeGB = free,
            SizeGB = total,
            AvailablePercent = free / total * 100,
            Name = drive.Name
        };
    }
}