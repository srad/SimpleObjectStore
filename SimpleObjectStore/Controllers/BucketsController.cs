using Microsoft.AspNetCore.Mvc;
using SimpleObjectStore.Filters;
using SimpleObjectStore.Models;
using SimpleObjectStore.Services.Interfaces;

namespace SimpleObjectStore.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
[Produces("application/json")]
[ApiKey]
public class BucketsController(IBucketsService service) : ControllerBase
{
    [HttpGet]
    public Task<IEnumerable<Bucket>> GetAsync() => service.ToListAsync();

    [HttpGet($"{{{nameof(name)}}}/name")]
    public Task<Bucket> GetByNameAsync(string name) => service.FindByNameAsync(name);

    [HttpGet($"{{{nameof(id)}}}/id")]
    public Task<Bucket> GetByIdAsync(string id) => service.FindById(id);

    [HttpGet($"exists/{{{nameof(name)}}}")]
    public Task<bool> BucketExistsAsync(string name) => service.ExistsAsync(name);

    [HttpPost($"{{{nameof(name)}}}")]
    public Task<Bucket> PostBucketAsync(string name) => service.CreateAsync(name);

    [HttpDelete($"{{{nameof(id)}}}")]
    public Task DeleteAsync(string id) => service.DeleteAsync(id);
}