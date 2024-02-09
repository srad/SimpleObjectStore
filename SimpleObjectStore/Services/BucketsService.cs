using Microsoft.EntityFrameworkCore;
using SimpleObjectStore.Helpers.Interfaces;
using SimpleObjectStore.Models;
using SimpleObjectStore.Services.Interfaces;

namespace SimpleObjectStore.Services;

public class BucketsService(ApplicationDbContext context, ISlug slug, ILogger<BucketsService> logger) : IBucketsService
{
    private readonly string _storagePath = Environment.GetEnvironmentVariable("STORAGE_DIRECTORY") ?? throw new MissingFieldException("storage directory missing");
    private string BucketPath(string directoryName) => Path.Combine(_storagePath, directoryName);

    public async Task<IEnumerable<Bucket>> ToListAsync() => await context.Buckets
        .Select(x => new Bucket
        {
            BucketId = x.BucketId,
            CreatedAt = x.CreatedAt,
            Size = x.Files.Count,
            DirectoryName = x.DirectoryName,
            Name = x.Name
        })
        .AsNoTracking()
        .ToListAsync();

    public async Task<Bucket> FindByNameAsync(string name)
    {
        var bucket = await context.Buckets
            .Include(x => x.Files)
            .Select(x => new Bucket
            {
                Files = x.Files,
                CreatedAt = x.CreatedAt,
                BucketId = x.BucketId,
                Name = x.Name,
                Size = x.Files.Count(),
                DirectoryName = x.DirectoryName,
            })
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Name == name);

        if (bucket == null)
        {
            throw new Exception("Bucket not found");
        }

        return bucket;
    }

    public async Task<Bucket> FindById(string id)
    {
        var bucket = await context.Buckets
            .Include(x => x.Files)
            .Select(x => new Bucket
            {
                Files = x.Files,
                CreatedAt = x.CreatedAt,
                BucketId = x.BucketId,
                Name = x.Name,
                Size = x.Files.Count(),
                DirectoryName = x.DirectoryName,
            })
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.BucketId == id);

        if (bucket == null)
        {
            throw new Exception("Bucket not found");
        }

        return bucket;
    }

    public async Task<Bucket> CreateAsync(string name)
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
            LastAccess = timestamp
        };

        Directory.CreateDirectory(BucketPath(bucket.DirectoryName));
        await context.Buckets.AddAsync(bucket);
        await context.SaveChangesAsync();

        return bucket;
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