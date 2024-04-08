using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleObjectStore.Models.DTO;
using SimpleObjectStore.Services.Interfaces;

namespace SimpleObjectStore.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
[Produces("application/json")]
[Authorize(Roles = "objectstore")]
public class BucketsController(IBucketsService service) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<BucketViewDto>> GetAsync() => service.ToListAsync();
    
    [HttpGet($"{{{nameof(name)}}}/name")]
    public Task<BucketViewDto> GetByNameAsync(string name) => service.FindByNameAsync(name);

    [HttpGet($"{{{nameof(id)}}}/id")]
    public Task<BucketViewDto> GetByIdAsync(string id) => service.FindById(id);

    [HttpGet($"exists/{{{nameof(name)}}}")]
    public Task<bool> ExistsAsync(string name) => service.ExistsAsync(name);

    [HttpPost($"{{{nameof(name)}}}")]
    public Task<BucketViewDto> CreateAsync(string name) => service.CreateAsync(name);

    [HttpDelete($"{{{nameof(id)}}}")]
    public Task DeleteAsync(string id) => service.DeleteAsync(id);
    
    [HttpPatch]
    public Task AsDownloadAsync(string id, bool asDownload) => service.AsDownloadAsync(id, asDownload);

}