using Microsoft.EntityFrameworkCore;
using SimpleObjectStore.Models;
using SimpleObjectStore.Services.Interfaces;

namespace SimpleObjectStore.Services;

public class ApiKeysService : IApiKeysService
{
    private readonly ApplicationDbContext _context;
    private readonly IKeyService _keyService;

    public ApiKeysService(ApplicationDbContext context, IKeyService keyService)
    {
        _context = context;
        _keyService = keyService;
    }
    
    public async Task<IEnumerable<ApiKey>> ToListAsync() => await _context.ApiKeys.ToListAsync();
    
    public async Task<ApiKey> CreateAsync(string title)
    {
        var key = new ApiKey
        {
            Key = _keyService.GenerateKey(), Title = title, CreatedAt = DateTimeOffset.UtcNow, AccessTimeLimited = false
        };

        await _context.ApiKeys.AddAsync(key);
        await _context.SaveChangesAsync();

        return key;
    }
    
    public async Task DeleteAsync(string key)
    {
        if (await _context.ApiKeys.CountAsync() == 1)
        {
            throw new Exception("At least one key must remain in database");
        }
        
        await _context.ApiKeys.Where(x => x.Key == key).ExecuteDeleteAsync();
    }
}