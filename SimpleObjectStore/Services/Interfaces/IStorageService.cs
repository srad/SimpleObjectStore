using Microsoft.AspNetCore.Mvc;
using SimpleObjectStore.Models;
using SimpleObjectStore.Models.DTO;

namespace SimpleObjectStore.Services.Interfaces;

public interface IStorageService
{
    Task<IEnumerable<BucketFile>> ToListAsync();
    Task<ActionResult<BucketFile>> FindByIdAsync(string id);
    Task<bool> ExistsAsync(string bucketId, string fileName);
    Task<List<CreateStorageFileResult>> SaveAsync(string bucketId, List<IFormFile> files);
    Task DeleteAsync(string id);
    Task PrivateAsync(string id);
    Task PublicAsync(string id);
    StorageStats GetStorageStatsAsync();
}