using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleObjectStore.Models;
using SimpleObjectStore.Services.Interfaces;

namespace SimpleObjectStore.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
[Produces("application/json")]
[Authorize(Roles = "objectstore")]
public class ApiKeysController(IApiKeysService service) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<ApiKey>> GetAsync() => service.ToListAsync();

    [HttpDelete]
    public Task DeleteAsync(string key) => service.DeleteAsync(key);

    [HttpPost]
    public Task<ApiKey> CreateAsync(string title) => service.CreateAsync(title);
}