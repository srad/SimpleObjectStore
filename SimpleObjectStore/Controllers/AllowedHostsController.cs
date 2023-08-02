using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimpleObjectStore.Filters;
using SimpleObjectStore.Models;
using SimpleObjectStore.Services;

namespace SimpleObjectStore.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
[Produces("application/json")]
[ApiKey]
public class AllowedHostsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public AllowedHostsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<List<AllowedHost>> GetKeysAsync() => await _context.AllowedHosts.ToListAsync();

    [HttpDelete($"{{{nameof(host)}}}")]
    public async Task<ActionResult> DeleteAsync(string host)
    {
        if (await _context.AllowedHosts.CountAsync() == 1)
        {
            return BadRequest("At least one host must remain in database");
        }

        await _context.AllowedHosts.Where(x => x.Hostname == host).ExecuteDeleteAsync();

        return Ok();
    }

    /// <summary>
    /// Adds a new white listed host name.
    /// </summary>
    /// <param name="host">Host name</param>
    /// <returns></returns>
    [HttpPost($"{{{nameof(host)}}}")]
    public async Task<ActionResult> PostAsync(string host)
    {
        // Validation
        host = host.Trim().ToLower();

        if (host.Contains("http"))
        {
            return BadRequest("Please provide only the hostname in the format 'host' without any prefixes or ports");
        }

        if (Uri.CheckHostName(host) == UriHostNameType.Unknown)
        {
            return BadRequest($"The hostname '{host}' is invalid");
        }

        if (await _context.AllowedHosts.AnyAsync(x => x.Hostname == host))
        {
            return BadRequest($"Hostname '{host}' already exists");
        }

        await _context.AllowedHosts.AddAsync(new AllowedHost { Hostname = host });
        await _context.SaveChangesAsync();

        return Ok();
    }
}