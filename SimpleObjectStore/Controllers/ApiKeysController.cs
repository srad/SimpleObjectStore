using Microsoft.AspNetCore.Mvc;
using SimpleObjectStore.Filters;
using SimpleObjectStore.Models;
using SimpleObjectStore.Services.Interfaces;

namespace SimpleObjectStore.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
[Produces("application/json")]
[ApiKey]
public class ApiKeysController : ControllerBase
{
    private readonly ILogger<ApiKeysController> _logger;
    private readonly IApiKeysService _service;

    public ApiKeysController(ILogger<ApiKeysController> logger, IApiKeysService service)
    {
        _logger = logger;
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ApiKey>>> GetKeysAsync()
    {
        try
        {
            return Ok(await _service.ToListAsync());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete]
    public async Task<ActionResult> DeleteAsync(string key)
    {
        try
        {
            await _service.DeleteAsync(key);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            return BadRequest(ex.Message);
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiKey>> CreateAsync(string title)
    {
        try
        {
            return Ok(await _service.CreateAsync(title));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            return BadRequest(ex.Message);
        }
    }
}