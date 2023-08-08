using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimpleObjectStore.Filters;
using SimpleObjectStore.Models;
using SimpleObjectStore.Services;

namespace SimpleObjectStore.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
[Produces("application/json")]
[ApiKey]
public class AllowedHostsController : ControllerBase
{
    private readonly AllowedHostsService _service;

    public AllowedHostsController(AllowedHostsService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<List<AllowedHost>> GetAsync() => await _service.ToListAsync();

    [HttpDelete($"{{{nameof(host)}}}")]
    public async Task DeleteAsync(string host) => await _service.DeleteAsync(host);

    /// <summary>
    /// Adds a new white listed host name.
    /// </summary>
    /// <param name="host"></param>
    /// <returns></returns>
    [HttpPost($"{{{nameof(host)}}}")]
    public async Task<AllowedHost> PostAsync(string host) => await _service.CreateAsync(host);
}