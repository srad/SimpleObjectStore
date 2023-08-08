using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using SimpleObjectStore.Filters;
using SimpleObjectStore.Models;
using SimpleObjectStore.Services.Interfaces;

namespace SimpleObjectStore.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
[Produces("application/json")]
[ApiKey]
public class BucketsController : ControllerBase
{
    private readonly ILogger<BucketsController> _logger;
    private readonly IBucketsService _service;

    public BucketsController(ILogger<BucketsController> logger, IBucketsService service)
    {
        _logger = logger;
        _service = service;
    }

    [HttpGet, OutputCache]
    public async Task<IEnumerable<Bucket>> GetAsync() => await _service.ToListAsync();

    [HttpGet($"{{{nameof(name)}}}/name"), OutputCache]
    public async Task<ActionResult<Bucket>> GetByNameAsync(string name)
    {
        try
        {
            return Ok(await _service.FindByNameAsync(name));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            return BadRequest(ex.Message);
        }
    }
    
    [HttpGet($"{{{nameof(id)}}}/id"), OutputCache]
    public async Task<ActionResult<Bucket>> GetByIdAsync(string id)
    {
        try
        {
            return Ok(await _service.FindById(id));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            return BadRequest(ex.Message);
        }
    }

    [HttpGet($"exists/{{{nameof(name)}}}"), OutputCache]
    public async Task<ActionResult<bool>> BucketExistsAsync(string name)
    {
        try
        {
            return Ok(await _service.ExistsAsync(name));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            return BadRequest(ex.Message);
        }
    }

    [HttpPost($"{{{nameof(name)}}}")]
    public async Task<ActionResult<Bucket>> PostBucketAsync(string name)
    {
        try
        {
            return Ok(await _service.CreateAsync(name));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete($"{{{nameof(id)}}}")]
    public async Task<ActionResult> DeleteAsync(string id)
    {
        try
        {
            await _service.DeleteAsync(id);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            return BadRequest(ex.Message);
        }
    }
}