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
public class ApiKeysController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ApiKeyService _apiKeyService;

    public ApiKeysController(ApplicationDbContext context, ApiKeyService apiKeyService)
    {
        _context = context;
        _apiKeyService = apiKeyService;
    }

    [HttpGet]
    public async Task<List<ApiKey>> GetKeysAsync() => await _context.ApiKeys.ToListAsync();

    [HttpDelete]
    public async Task<ActionResult> DeleteAsync(string key)
    {
        if (await _context.ApiKeys.CountAsync() == 1)
        {
            return BadRequest("At least one key must remain in database");
        }
        
        await _context.ApiKeys.Where(x => x.Key == key).ExecuteDeleteAsync();

        return Ok();
    }

    [HttpPost]
    public async Task<ApiKey> CreateAsync(string title)
    {
        var key = new ApiKey
        {
            Key = _apiKeyService.GenerateApiKey(), Title = title, CreatedAt = DateTimeOffset.UtcNow, AccessTimeLimited = false
        };

        await _context.ApiKeys.AddAsync(key);
        await _context.SaveChangesAsync();

        return key;
    }
}