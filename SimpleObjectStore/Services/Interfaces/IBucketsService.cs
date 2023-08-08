using Microsoft.AspNetCore.Mvc;
using SimpleObjectStore.Models;

namespace SimpleObjectStore.Services.Interfaces;

public interface IBucketsService
{
    Task<IEnumerable<Bucket>> ToListAsync();
    Task<Bucket> FindByNameAsync(string name);
    Task<Bucket> FindById(string id);
    Task<ActionResult<Bucket>> CreateAsync(string name);
    Task DeleteAsync(string id);
    Task<bool> ExistsAsync(string name);
}