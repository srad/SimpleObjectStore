using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using SimpleObjectStore.Filters;
using SimpleObjectStore.Models;
using SimpleObjectStore.Models.DTO;
using Slugify;

namespace SimpleObjectStore.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
[Produces("application/json")]
[ApiKey]
public class BucketsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly string _storagePath;
    private readonly ILogger<BucketsController> _logger;

    private string BucketPath(string directoryName) => Path.Combine(_storagePath, directoryName);

    public BucketsController(ApplicationDbContext context, ILogger<BucketsController> logger)
    {
        _context = context;
        _storagePath = Environment.GetEnvironmentVariable("STORAGE_DIRECTORY") ?? throw new MissingFieldException("storage directory missing");
        _logger = logger;
    }

    [HttpGet, OutputCache]
    public async Task<ActionResult<IEnumerable<Bucket>>> GetAsync()
    {
        return await _context.Buckets
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
    }

    [HttpGet($"{{{nameof(id)}}}"), OutputCache]
    public async Task<ActionResult<Bucket>> GetAsync(string id)
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
            return NotFound("Bucket not found");
        }

        return bucket;
    }

    [HttpGet($"exists/{{{nameof(name)}}}")]
    public async Task<bool> BucketExistsAsync(string name) => await _context.Buckets.AnyAsync(x => x.Name == name);

    [HttpPost]
    public async Task<ActionResult<Bucket>> PostAsync([FromBody] CreateBucket createBucket)
    {
        try
        {
            var helper = new SlugHelper();
            var slug = helper.GenerateSlug(createBucket.Name);

            if (await _context.Buckets.AnyAsync(x => x.Name == createBucket.Name || x.DirectoryName == slug))
            {
                throw new Exception($"The bucket name '{createBucket.Name}' is already in use");
            }

            var timestamp = DateTimeOffset.Now;
            var bucket = new Bucket
            {
                BucketId = Guid.NewGuid().ToString(),
                Name = createBucket.Name,
                DirectoryName = slug,
                CreatedAt = timestamp,
                LastAccess = timestamp
            };

            Directory.CreateDirectory(BucketPath(bucket.DirectoryName));
            await _context.Buckets.AddAsync(bucket);
            await _context.SaveChangesAsync();
            return Ok(bucket);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return BadRequest(e.Message);
        }
    }

    [HttpDelete($"{{{nameof(id)}}}")]
    public async Task<IActionResult> DeleteAsync(string id)
    {
        try
        {
            var bucket = await _context.Buckets
                .Include(x => x.Files)
                .FirstOrDefaultAsync(x => x.BucketId == id);

            if (bucket == null)
            {
                return NotFound("Bucket not found");
            }

            Directory.Delete(BucketPath(bucket.DirectoryName), true);

            await _context.Buckets
                .Where(x => x.BucketId == id)
                .ExecuteDeleteAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return BadRequest(e.Message);
        }

        return Ok();
    }
}