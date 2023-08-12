using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi.Models;
using SimpleObjectStore.Filters;
using SimpleObjectStore.Helpers;
using SimpleObjectStore.Helpers.Interfaces;
using SimpleObjectStore.Http;
using SimpleObjectStore.Models;
using SimpleObjectStore.Seeds;
using SimpleObjectStore.Services;
using SimpleObjectStore.Services.Interfaces;

// Handle data folders and paths.

var appDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
var defaultAppDirectory = Path.Combine(appDirectory, "SimpleObjectStore");

if (Environment.GetEnvironmentVariable("STORAGE_DIRECTORY") == null)
{
    var defaultStoragePath = Path.Combine(defaultAppDirectory, "storage");
    Environment.SetEnvironmentVariable("STORAGE_DIRECTORY", defaultStoragePath);
}

if (Environment.GetEnvironmentVariable("DB_PATH") == null)
{
    var defaultDbPath = Path.Combine(defaultAppDirectory, "data.db");
    Environment.SetEnvironmentVariable("DB_PATH", defaultDbPath);
}

var storageDirectory = Environment.GetEnvironmentVariable("STORAGE_DIRECTORY");
if (storageDirectory == null)
{
    throw new ArgumentNullException("STORAGE_DIRECTORY missing");
}

if (!Directory.Exists(storageDirectory))
{
    Directory.CreateDirectory(storageDirectory);
}

// Build application.

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options => { options.UseSqlite($"Data Source={Environment.GetEnvironmentVariable("DB_PATH")}"); });
builder.Services.AddScoped<ApiKeyService>();
builder.Services.AddScoped<ApiKeyAuthorizationFilter>();
builder.Services.AddScoped<IApiKeyValidator, ApiKeyValidator>();
builder.Services.AddScoped<StorageNameValidator>();
builder.Services.AddScoped<ISlug, StorageSlug>();
builder.Services.AddScoped<IAllowedHostsService, AllowedHostsService>();
builder.Services.AddScoped<IBucketsService, BucketsService>();
builder.Services.AddScoped<IStorageService, StorageService>();

builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new OpenApiInfo { Version = "v1", Title = "SimpleObjectStore API" }); });

builder.Services.AddOutputCache(options => { options.AddBasePolicy(b => b.Cache()); });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseDbInit();

//app.UseHttpsRedirection();
app.UseAuthorization();
//app.UseAuthentication();
app.MapControllers();

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(storageDirectory),
    RequestPath = "",
    OnPrepareResponse = ctx => StaticFileOptionsHandler.OnPrepareResponse(app, ctx)
});

app.Run();