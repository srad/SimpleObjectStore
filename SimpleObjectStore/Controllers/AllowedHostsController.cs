using Microsoft.AspNetCore.Mvc;
using SimpleObjectStore.Filters;
using SimpleObjectStore.Models;
using SimpleObjectStore.Services.Interfaces;

namespace SimpleObjectStore.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
[Produces("application/json")]
[ApiKey]
public class AllowedHostsController : ControllerBase
{
    private readonly IAllowedHostsService _service;
    private readonly ILogger<AllowedHostsController> _logger;

    public AllowedHostsController(IAllowedHostsService service, ILogger<AllowedHostsController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AllowedHost>>> GetAsync() => Ok(await _service.ToListAsync());

    [HttpDelete($"{{{nameof(host)}}}")]
    public async Task<IActionResult> DeleteAsync(string host)
    {
        try
        {
            await _service.DeleteAsync(host);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Adds a new white listed host name.
    /// </summary>
    /// <param name="host"></param>
    /// <returns></returns>
    [HttpPost($"{{{nameof(host)}}}")]
    public async Task<ActionResult<AllowedHost>> PostAsync(string host)
    {
        try
        {
            return Ok(await _service.CreateAsync(host));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            return BadRequest(ex.Message);
        }
    }
}