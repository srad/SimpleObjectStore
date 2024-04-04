using Microsoft.AspNetCore.Mvc;
using SimpleObjectStore.Filters;
using SimpleObjectStore.Models;
using SimpleObjectStore.Models.DTO;
using SimpleObjectStore.Services.Interfaces;

namespace SimpleObjectStore.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
[Produces("application/json")]
[ApiKey]
public class StorageController(IStorageService service) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<BucketFile>> GetFilesAsync() => service.ToListAsync();

    [HttpGet($"{{{nameof(id)}}}")]
    public Task<BucketFile> GetFileAsync(string id) => service.FindByIdAsync(id);

    [HttpGet($"itemexists/{{{nameof(bucketId)}}}/{{{nameof(fileName)}}}")]
    public Task<bool> ExistsAsync(string bucketId, string fileName) => service.ExistsAsync(bucketId, fileName);

    [HttpPost("{bucketId}")]
    public Task<IReadOnlyList<CreateStorageFileResult>> SaveFileAsync(string bucketId, [FromForm] List<IFormFile> files) => service.SaveAsync(bucketId, files);

    [HttpDelete($"{{{nameof(id)}}}")]
    public Task DeleteAsync(string id) => service.DeleteAsync(id);

    [HttpPost("private")]
    public Task PrivateAsync(string id) => service.PrivateAsync(id);

    [HttpPost("public")]
    public Task PublicAsync(string id) => service.PublicAsync(id);

    [HttpGet("storageInfo")]
    public StorageStats GetInfoAsync() => service.GetStorageStatsAsync();
}