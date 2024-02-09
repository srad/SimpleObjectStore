using Microsoft.EntityFrameworkCore;
using SimpleObjectStore.Models;
using SimpleObjectStore.Services.Interfaces;

namespace SimpleObjectStore.Services;

public class ApiKeysService(ApplicationDbContext context, IKeyService keyService) : IApiKeysService
{
    public async Task<IEnumerable<ApiKey>> ToListAsync() => await context.ApiKeys.ToListAsync();
    
    public async Task<ApiKey> CreateAsync(string title)
    {
        var key = new ApiKey
        {
            Key = keyService.GenerateKey(), Title = title, CreatedAt = DateTimeOffset.UtcNow, AccessTimeLimited = false
        };

        await context.ApiKeys.AddAsync(key);
        await context.SaveChangesAsync();

        return key;
    }
    
    public async Task DeleteAsync(string key)
    {
        if (await context.ApiKeys.CountAsync() == 1)
        {
            throw new Exception("At least one key must remain in database");
        }
        
        await context.ApiKeys.Where(x => x.Key == key).ExecuteDeleteAsync();
    }
}