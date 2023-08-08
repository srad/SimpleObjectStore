using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using SimpleObjectStore.Filters;
using SimpleObjectStore.Models;
using SimpleObjectStore.Models.DTO;
using SimpleObjectStore.Services.Interfaces;

namespace SimpleObjectStore.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
[Produces("application/json")]
[ApiKey]
public class StorageController : ControllerBase
{
    private readonly ILogger<StorageController> _logger;
    private readonly IStorageService _service;

    public StorageController(ILogger<StorageController> logger, IStorageService service)
    {
        _logger = logger;
        _service = service;
    }

    [HttpGet, OutputCache]
    public async Task<ActionResult<IEnumerable<BucketFile>>> GetFiles()
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

    [HttpGet($"{{{nameof(id)}}}"), OutputCache]
    public async Task<ActionResult<BucketFile>> GetStorageFile(string id)
    {
        try
        {
            return Ok(await _service.FindByIdAsync(id));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            return BadRequest(ex.Message);
        }
    }

    [HttpGet($"itemexists/{{{nameof(bucketId)}}}/{{{nameof(fileName)}}}")]
    public async Task<ActionResult<bool>> ExistsAsync(string bucketId, string fileName)
    {
        try
        {
            return Ok(await _service.ExistsAsync(bucketId, fileName));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{bucketId}")]
    public async Task<ActionResult<List<CreateStorageFileResult>>> PostStorageFileAsync(string bucketId, [FromForm] List<IFormFile> files)
    {
        try
        {
            return Ok(await _service.SaveAsync(bucketId, files));
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

    [HttpPost("private")]
    public async Task<ActionResult> PrivateAsync(string id)
    {
        try
        {
            await _service.PrivateAsync(id);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("public")]
    public async Task<ActionResult> PublicAsync(string id)
    {
        try
        {
            await _service.PublicAsync(id);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("storageInfo"), OutputCache]
    public ActionResult<StorageStats> GetStorageInfo()
    {
        try
        {
            return Ok(_service.GetStorageStatsAsync());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            return BadRequest(ex.Message);
        }
    }
}