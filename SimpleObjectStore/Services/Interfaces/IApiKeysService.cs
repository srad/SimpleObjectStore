using SimpleObjectStore.Models;

namespace SimpleObjectStore.Services.Interfaces;

public interface IApiKeysService
{
    Task<IReadOnlyList<ApiKey>> ToListAsync();
    Task<ApiKey> CreateAsync(string title);
    Task DeleteAsync(string key);
}