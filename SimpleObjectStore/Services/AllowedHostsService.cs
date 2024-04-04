using Microsoft.EntityFrameworkCore;
using SimpleObjectStore.Models;
using SimpleObjectStore.Services.Interfaces;

namespace SimpleObjectStore.Services;

public class AllowedHostsService(ApplicationDbContext context) : IAllowedHostsService
{
    public async Task DeleteAsync(string host)
    {
        if (await context.AllowedHosts.CountAsync() == 1)
        {
            throw new Exception("At least one host must remain in database");
        }

        await context.AllowedHosts.Where(x => x.Hostname == host).ExecuteDeleteAsync();
    }
    
    public async Task<IReadOnlyList<AllowedHost>> ToListAsync() => await context.AllowedHosts.ToListAsync();

    public async Task<AllowedHost> CreateAsync(string host)
    {
        // Validation
        host = host.Trim().ToLower();

        if (host.Contains("http"))
        {
            throw new Exception("Please provide only the hostname in the format 'host' without any prefixes or ports");
        }

        if (Uri.CheckHostName(host) == UriHostNameType.Unknown)
        {
            throw new Exception($"The hostname '{host}' is invalid");
        }

        if (await context.AllowedHosts.AnyAsync(x => x.Hostname == host))
        {
            throw new Exception($"Hostname '{host}' already exists");
        }

        var newHost = new AllowedHost { Hostname = host };
        await context.AllowedHosts.AddAsync(newHost);
        await context.SaveChangesAsync();

        return newHost;
    }
}