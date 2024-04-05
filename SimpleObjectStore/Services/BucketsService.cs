using Microsoft.EntityFrameworkCore;
using SimpleObjectStore.Helpers.Interfaces;
using SimpleObjectStore.Models;
using SimpleObjectStore.Models.DTO;
using SimpleObjectStore.Services.Interfaces;

namespace SimpleObjectStore.Services;

/// <summary>
/// Could optionally also use a storage strategy.
/// </summary>
/// <param name="context"></param>
/// <param name="slug"></param>
/// <param name="accessor"></param>
public class BucketsService(ApplicationDbContext context, ISlug slug, IHttpContextAccessor _httpContextAccessor) : IBucketsService
{
    private readonly string _storagePath = Environment.GetEnvironmentVariable("STORAGE_DIRECTORY") ?? throw new MissingFieldException("storage directory missing");
    private readonly string _url = $"{_httpContextAccessor.HttpContext.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host}{_httpContextAccessor.HttpContext.Request.PathBase}";

    private string BucketPath(string directoryName) => Path.Combine(_storagePath, directoryName);

    public async Task<IReadOnlyList<BucketViewDto>> ToListAsync() => await context.Buckets
        .AsNoTracking()
        .Select(x => new BucketViewDto
        {
            BucketId = x.BucketId,
            CreatedAt = x.CreatedAt,
            LastAccess = default,
            FileCount = x.Files.Count,
            Private = x.Private,
            Files = null,
            DirectoryName = x.DirectoryName,
            Name = x.Name,
        })
        .ToListAsync();

    public Task<BucketViewDto> FindByNameAsync(string name) => context.Buckets
        .AsNoTracking()
        .Include(x => x.Files)
        .Select(bucket => new BucketViewDto
        {
            CreatedAt = bucket.CreatedAt,
            BucketId = bucket.BucketId,
            Name = bucket.Name,
            FileCount = bucket.Files.Count,
            DirectoryName = bucket.DirectoryName,

            Files = bucket.Files.Select(file => new FileViewDto
            {
                FileName = file.FileName,
                RelativeUrl = file.Url,
                AbsoluteUrl = _url + file.Url,
                CreatedAt = file.CreatedAt,
                FileSizeMB = file.FileSizeMB,
                FileSize = file.FileSize,
                AccessCount = file.AccessCount,
                LastAccess = default,
                Private = false,
                StorageFileId = file.StorageFileId,
            }).ToList(),
        })
        .FirstAsync(x => x.Name == name);

    public Task<BucketViewDto> FindById(string id) => context.Buckets
        .AsNoTracking()
        .Include(bucket => bucket.Files)
        .Select(bucket => new BucketViewDto
        {
            CreatedAt = bucket.CreatedAt,
            BucketId = bucket.BucketId,
            Name = bucket.Name,
            FileCount = bucket.Files.Count(),
            DirectoryName = bucket.DirectoryName,
            Files = bucket.Files.Select(file => new FileViewDto
            {
                FileName = file.FileName,
                RelativeUrl = file.Url,
                CreatedAt = file.CreatedAt,
                FileSizeMB = file.FileSizeMB,
                LastAccess = file.LastAccess,
                Private = file.Private,
                StorageFileId = file.StorageFileId,
                FileSize = file.FileSize,
                AccessCount = file.AccessCount,
                AbsoluteUrl = $"{_url}/{file.Url}",
            }).ToList(),
        })
        .FirstAsync(x => x.BucketId == id);

    public async Task<BucketViewDto> CreateAsync(string name)
    {
        var slug1 = slug.Generate(name);

        if (await context.Buckets.AnyAsync(x => x.Name == name || x.DirectoryName == slug1))
        {
            throw new Exception($"The bucket name '{name}' is already in use");
        }

        var timestamp = DateTimeOffset.Now;
        var bucket = new Bucket
        {
            BucketId = Guid.NewGuid().ToString(),
            Name = name,
            DirectoryName = slug1,
            CreatedAt = timestamp,
            LastAccess = timestamp,
        };

        Directory.CreateDirectory(BucketPath(bucket.DirectoryName));

        await context.Buckets.AddAsync(bucket);
        await context.SaveChangesAsync();

        return new BucketViewDto
        {
            BucketId = bucket.BucketId,
            Name = bucket.Name,
            DirectoryName = bucket.DirectoryName,
            CreatedAt = bucket.CreatedAt,
            LastAccess = bucket.LastAccess,
            FileCount = bucket.Size,
            Private = bucket.Private,
        };
    }

    public async Task DeleteAsync(string id)
    {
        var bucket = await context.Buckets
            .Include(x => x.Files)
            .FirstOrDefaultAsync(x => x.BucketId == id);

        if (bucket == null)
        {
            throw new Exception("Bucket not found");
        }

        Directory.Delete(BucketPath(bucket.DirectoryName), true);

        await context.Buckets
            .Where(x => x.BucketId == id)
            .ExecuteDeleteAsync();
    }

    public async Task<bool> ExistsAsync(string name) => await context.Buckets.AnyAsync(x => x.Name == name);
}