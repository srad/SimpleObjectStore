using SimpleObjectStore.Models;

namespace SimpleObjectStore.Services.Interfaces;

public interface IAllowedHostsService
{
    Task DeleteAsync(string host);
    Task<IEnumerable<AllowedHost>> ToListAsync();
    Task<AllowedHost> CreateAsync(string host);
}