using Microsoft.AspNetCore.Mvc;
using SimpleObjectStore.Filters;
using SimpleObjectStore.Models;
using SimpleObjectStore.Services.Interfaces;

namespace SimpleObjectStore.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
[Produces("application/json")]
[ApiKey]
public class ApiKeysController(IApiKeysService service) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<ApiKey>> GetKeysAsync() => service.ToListAsync();

    [HttpDelete]
    public Task DeleteAsync(string key) => service.DeleteAsync(key);

    [HttpPost]
    public Task<ApiKey> CreateAsync(string title) => service.CreateAsync(title);
}