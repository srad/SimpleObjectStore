using SimpleObjectStore.Models;

namespace SimpleObjectStore.Services.Interfaces;

public interface IAllowedHostsService
{
    Task DeleteAsync(string host);
    Task<IReadOnlyList<AllowedHost>> ToListAsync();
    Task<AllowedHost> CreateAsync(string host);
}