using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimpleObjectStore.Helpers;
using SimpleObjectStore.Models;
using SimpleObjectStore.Models.DTO;
using SimpleObjectStore.Services.Interfaces;

namespace SimpleObjectStore.Services;

public class StorageService : IStorageService
{
    private readonly ApplicationDbContext _context;
    private readonly ISlug _slug;
    private readonly ILogger<StorageService> _logger;
    private readonly string _storagePath;
    private string BucketPath(string directoryName) => Path.Combine(_storagePath, directoryName);

    public StorageService(ApplicationDbContext context, ISlug slug, ILogger<StorageService> logger)
    {
        _context = context;
        _slug = slug;
        _logger = logger;
        _storagePath = Environment.GetEnvironmentVariable("STORAGE_DIRECTORY") ?? throw new ArgumentNullException();
    }

    public async Task<IEnumerable<BucketFile>> ToListAsync() => await _context.BucketFiles.AsNoTracking().ToListAsync();

    public async Task<ActionResult<BucketFile>> FindByIdAsync(string id)
    {
        var storageFile = await _context.BucketFiles.FindAsync(id);

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
    public async Task<bool> ExistsAsync(string bucketId, string fileName) => await _context.BucketFiles.AnyAsync(x => x.BucketId == bucketId && x.StoredFileName == _slug.Generate(fileName));

    public async Task<List<CreateStorageFileResult>> SaveAsync(string bucketId, List<IFormFile> files)
    {
        var bucket = await _context.Buckets
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
                var fileNameSlug = _slug.Generate(file.FileName);
                var fileExists = await _context.BucketFiles.AnyAsync(x => x.BucketId == bucketId && x.StoredFileName == fileNameSlug);
                if (fileExists)
                {
                    results.Add(new CreateStorageFileResult
                    {
                        FileName = file.FileName,
                        StorageFile = await _context.BucketFiles.FirstAsync(x => x.BucketId == bucketId && x.StoredFileName == fileNameSlug),
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
                await _context.BucketFiles.AddAsync(storage);
                await _context.SaveChangesAsync();
                results.Add(new CreateStorageFileResult()
                {
                    FileName = file.FileName,
                    StorageFile = storage,
                    Success = true,
                });
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
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
        var storageFile = await _context.BucketFiles.FindAsync(id);
        if (storageFile == null)
        {
            throw new Exception("File not found");
        }

        try
        {
            _logger.LogInformation("Deleting file '{FilePath}'", storageFile.FilePath);
            File.Delete(storageFile.FilePath);
            _context.BucketFiles.Remove(storageFile);
            await _context.SaveChangesAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            throw new Exception($"Error deleting '{storageFile.FilePath}': {e.Message}");
        }
    }

    public async Task PrivateAsync(string id) => await _context.BucketFiles
        .Where(x => x.StorageFileId == id)
        .ExecuteUpdateAsync(x => x.SetProperty(p => p.Private, true));

    public async Task PublicAsync(string id) => await _context.BucketFiles
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