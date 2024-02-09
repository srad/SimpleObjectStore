using SimpleObjectStore.Models;

namespace SimpleObjectStore.Services.Interfaces;

public interface IBucketsService
{
    Task<IEnumerable<Bucket>> ToListAsync();
    Task<Bucket> FindByNameAsync(string name);
    Task<Bucket> FindById(string id);
    Task<Bucket> CreateAsync(string name);
    Task DeleteAsync(string id);
    Task<bool> ExistsAsync(string name);
}