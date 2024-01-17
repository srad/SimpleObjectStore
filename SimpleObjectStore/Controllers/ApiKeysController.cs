using Microsoft.AspNetCore.Mvc;
using SimpleObjectStore.Filters;
using SimpleObjectStore.Models;
using SimpleObjectStore.Services.Interfaces;

namespace SimpleObjectStore.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
[Produces("application/json")]
[ApiKey]
public class ApiKeysController(ILogger<ApiKeysController> logger, IApiKeysService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ApiKey>>> GetKeysAsync()
    {
        try
        {
            return Ok(await service.ToListAsync());
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete]
    public async Task<ActionResult> DeleteAsync(string key)
    {
        try
        {
            await service.DeleteAsync(key);
            return Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            return BadRequest(ex.Message);
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiKey>> CreateAsync(string title)
    {
        try
        {
            return Ok(await service.CreateAsync(title));
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            return BadRequest(ex.Message);
        }
    }
}