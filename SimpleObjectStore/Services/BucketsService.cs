using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimpleObjectStore.Helpers;
using SimpleObjectStore.Models;
using SimpleObjectStore.Services.Interfaces;

namespace SimpleObjectStore.Services;

public class BucketsService : IBucketsService
{
    private readonly ApplicationDbContext _context;
    private readonly ISlug _slug;
    private readonly string _storagePath;
    private readonly ILogger<BucketsService> _logger;
    private string BucketPath(string directoryName) => Path.Combine(_storagePath, directoryName);

    public BucketsService(ApplicationDbContext context, ISlug slug, ILogger<BucketsService> logger)
    {
        _context = context;
        _slug = slug;
        _storagePath = Environment.GetEnvironmentVariable("STORAGE_DIRECTORY") ?? throw new MissingFieldException("storage directory missing");
        _logger = logger;
    }

    public async Task<IEnumerable<Bucket>> ToListAsync() => await _context.Buckets
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
        var bucket = await _context.Buckets
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
        var bucket = await _context.Buckets
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

    public async Task<ActionResult<Bucket>> CreateAsync(string name)
    {
        try
        {
            var slug = _slug.Generate(name);

            if (await _context.Buckets.AnyAsync(x => x.Name == name || x.DirectoryName == slug))
            {
                throw new Exception($"The bucket name '{name}' is already in use");
            }

            var timestamp = DateTimeOffset.Now;
            var bucket = new Bucket
            {
                BucketId = Guid.NewGuid().ToString(),
                Name = name,
                DirectoryName = slug,
                CreatedAt = timestamp,
                LastAccess = timestamp
            };

            Directory.CreateDirectory(BucketPath(bucket.DirectoryName));
            await _context.Buckets.AddAsync(bucket);
            await _context.SaveChangesAsync();
            return bucket;
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            throw;
        }
    }

    public async Task DeleteAsync(string id)
    {
        try
        {
            var bucket = await _context.Buckets
                .Include(x => x.Files)
                .FirstOrDefaultAsync(x => x.BucketId == id);

            if (bucket == null)
            {
                throw new Exception("Bucket not found");
            }

            Directory.Delete(BucketPath(bucket.DirectoryName), true);

            await _context.Buckets
                .Where(x => x.BucketId == id)
                .ExecuteDeleteAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            throw;
        }
    }

    public async Task<bool> ExistsAsync(string name) => await _context.Buckets.AnyAsync(x => x.Name == name);
}