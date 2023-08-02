using Microsoft.EntityFrameworkCore;
using SimpleObjectStore.Seeds;
using SimpleObjectStore.Services;

namespace SimpleObjectStore.Models;

public class ApplicationDbContext : DbContext
{
    public DbSet<ApiKey> ApiKeys { get; set; }
    public DbSet<BucketFile> BucketFiles { get; set; }
    public DbSet<Bucket> Buckets { get; set; }
    public DbSet<AllowedHost> AllowedHosts { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }
}