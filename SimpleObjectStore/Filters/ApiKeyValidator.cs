using Microsoft.EntityFrameworkCore;
using SimpleObjectStore.Models;

namespace SimpleObjectStore.Filters;

public class ApiKeyValidator : IApiKeyValidator
{
    private readonly ApplicationDbContext _context;

    public ApiKeyValidator(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> IsValid(string apiKey)
    {
        return await _context.ApiKeys.AnyAsync(x => x.Key == apiKey);
    }
}

public interface IApiKeyValidator
{
    public Task<bool> IsValid(string apiKey);
}