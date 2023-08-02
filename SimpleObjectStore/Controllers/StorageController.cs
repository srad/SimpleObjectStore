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
public class StorageController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly string _storagePath;
    private readonly ILogger<StorageController> _logger;
    private readonly SlugHelper _slugHelper;
    private string BucketPath(string directoryName) => Path.Combine(_storagePath, directoryName);

    public StorageController(ApplicationDbContext context, ILogger<StorageController> logger)
    {
        _context = context;
        _storagePath = Environment.GetEnvironmentVariable("STORAGE_DIRECTORY") ?? throw new ArgumentNullException();
        _logger = logger;
        _slugHelper = new SlugHelper();
    }

    [HttpGet, OutputCache]
    public async Task<ActionResult<IEnumerable<BucketFile>>> GetFiles()
    {
        return await _context.BucketFiles.AsNoTracking().ToListAsync();
    }

    [HttpGet($"{{{nameof(id)}}}"), OutputCache]
    public async Task<ActionResult<BucketFile>> GetStorageFile(string id)
    {
        var storageFile = await _context.BucketFiles.FindAsync(id);

        if (storageFile == null)
        {
            return NotFound();
        }

        return Ok(storageFile);
    }

    /// <summary>
    /// Notice that this function slugs the filename before comparing it.
    /// It does no raw comparison of names.
    /// </summary>
    /// <param name="bucketId"></param>
    /// <param name="fileName"></param>
    /// <returns></returns>
    [HttpGet($"exists/{{{nameof(bucketId)}}}/{{{nameof(fileName)}}}")]
    public async Task<bool> FileExistsAsync(string bucketId, string fileName) => await _context.BucketFiles.AnyAsync(x => x.BucketId == bucketId && x.StoredFileName == _slugHelper.GenerateSlug(fileName));

    [HttpPost("{bucketId}")]
    public async Task<ActionResult<List<CreateStorageFileResult>>> PostStorageFile(string bucketId, [FromForm] IEnumerable<IFormFile> files)
    {
        var bucket = await _context.Buckets
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.BucketId == bucketId);

        if (bucket == null)
        {
            return NotFound("Bucket not found");
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
                var fileNameSlug = _slugHelper.GenerateSlug(file.FileName);
                var fileExists = await _context.BucketFiles.AnyAsync(x => x.BucketId == bucketId && x.StoredFileName == fileNameSlug);
                if (fileExists)
                {
                    results.Add(new CreateStorageFileResult
                    {
                        FileName = file.FileName,
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
                    ContentType = file.ContentType,
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
                    Success = true
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

        return Ok(results);
    }

    [HttpDelete($"{{{nameof(id)}}}")]
    public async Task<IActionResult> DeleteAsync(string id)
    {
        var storageFile = await _context.BucketFiles.FindAsync(id);
        if (storageFile == null)
        {
            return NotFound("File not found");
        }

        try
        {
            _logger.LogInformation("Deleting file '{file}'", storageFile.FilePath);
            System.IO.File.Delete(storageFile.FilePath);
            _context.BucketFiles.Remove(storageFile);
            await _context.SaveChangesAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return BadRequest($"Error deleting '{storageFile.FilePath}': {e.Message}");
        }

        return Ok();
    }

    [HttpPost("private")]
    public async Task PrivateAsync(string id) => await _context.BucketFiles
        .Where(x => x.StorageFileId == id)
        .ExecuteUpdateAsync(x => x.SetProperty(p => p.Private, true));

    [HttpPost("public")]
    public async Task PublicAsync(string id) => await _context.BucketFiles
        .Where(x => x.StorageFileId == id)
        .ExecuteUpdateAsync(x => x.SetProperty(p => p.Private, false));

    [HttpGet("storageInfo"), OutputCache]
    public StorageInfo GetStorageInfo()
    {
        var drive = new DriveInfo(_storagePath);
        var free = (float)drive.TotalFreeSpace / 1024 / 1024 / 1024;
        var total = (float)drive.TotalSize / 1024 / 1024 / 1024;

        return new StorageInfo
        {
            FreeGB = free,
            SizeGB = total,
            AvailablePercent = free / total * 100,
            Name = drive.Name
        };
    }
}