using Microsoft.AspNetCore.Mvc;
using SimpleObjectStore.Filters;
using SimpleObjectStore.Models;
using SimpleObjectStore.Services.Interfaces;

namespace SimpleObjectStore.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
[Produces("application/json")]
[ApiKey]
public class AllowedHostsController(IAllowedHostsService service, ILogger<AllowedHostsController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AllowedHost>>> GetAsync() => Ok(await service.ToListAsync());

    [HttpDelete($"{{{nameof(host)}}}")]
    public async Task<IActionResult> DeleteAsync(string host)
    {
        try
        {
            await service.DeleteAsync(host);
            return Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
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
            return Ok(await service.CreateAsync(host));
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            return BadRequest(ex.Message);
        }
    }
}