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
public class BucketsController(ILogger<BucketsController> logger, IBucketsService service) : ControllerBase
{
    [HttpGet, OutputCache]
    public async Task<IEnumerable<Bucket>> GetAsync() => await service.ToListAsync();

    [HttpGet($"{{{nameof(name)}}}/name"), OutputCache]
    public async Task<ActionResult<Bucket>> GetByNameAsync(string name)
    {
        try
        {
            return Ok(await service.FindByNameAsync(name));
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            return BadRequest(ex.Message);
        }
    }
    
    [HttpGet($"{{{nameof(id)}}}/id"), OutputCache]
    public async Task<ActionResult<Bucket>> GetByIdAsync(string id)
    {
        try
        {
            return Ok(await service.FindById(id));
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            return BadRequest(ex.Message);
        }
    }

    [HttpGet($"exists/{{{nameof(name)}}}"), OutputCache]
    public async Task<ActionResult<bool>> BucketExistsAsync(string name)
    {
        try
        {
            return Ok(await service.ExistsAsync(name));
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            return BadRequest(ex.Message);
        }
    }

    [HttpPost($"{{{nameof(name)}}}")]
    public async Task<ActionResult<Bucket>> PostBucketAsync(string name)
    {
        try
        {
            return Ok(await service.CreateAsync(name));
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete($"{{{nameof(id)}}}")]
    public async Task<ActionResult> DeleteAsync(string id)
    {
        try
        {
            await service.DeleteAsync(id);
            return Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            return BadRequest(ex.Message);
        }
    }
}