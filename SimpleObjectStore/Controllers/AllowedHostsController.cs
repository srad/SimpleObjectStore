﻿using Microsoft.AspNetCore.Mvc;
using SimpleObjectStore.Filters;
using SimpleObjectStore.Models;
using SimpleObjectStore.Services.Interfaces;

namespace SimpleObjectStore.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
[Produces("application/json")]
[ApiKey]
public class AllowedHostsController(IAllowedHostsService service) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<AllowedHost>> GetAsync() => service.ToListAsync();

    [HttpDelete($"{{{nameof(host)}}}")]
    public Task DeleteAsync(string host) => service.DeleteAsync(host);

    /// <summary>
    /// Adds a new white listed host name.
    /// </summary>
    /// <param name="host"></param>
    /// <returns></returns>
    [HttpPost($"{{{nameof(host)}}}")]
    public Task<AllowedHost> PostAsync(string host) =>  service.CreateAsync(host);
}