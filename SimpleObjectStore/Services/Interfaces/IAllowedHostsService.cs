using SimpleObjectStore.Models;

namespace SimpleObjectStore.Services.Interfaces;

public interface IAllowedHostsService
{
    Task DeleteAsync(string host);
    Task<List<AllowedHost>> ToListAsync();
    Task<AllowedHost> CreateAsync(string host);
}