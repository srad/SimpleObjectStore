using System.Security.Claims;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SimpleObjectStore.Auth;
using SimpleObjectStore.Helpers;
using SimpleObjectStore.Helpers.Interfaces;
using SimpleObjectStore.Http;
using SimpleObjectStore.Models;
using SimpleObjectStore.Models.Configs;
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

Console.WriteLine($"DB directory: {Environment.GetEnvironmentVariable("DB_PATH")}");
Console.WriteLine($"Storage directory: {storageDirectory}");

// Build application.

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddApiKeySupport(options =>
    {
    })
    .AddJwtBearer(options =>
    {
        var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtConfig>();

        options.Authority = jwtSettings!.Authority;
        options.RequireHttpsMetadata = builder.Environment.IsProduction();
        options.Audience = "account";
        options.IncludeErrorDetails = !builder.Environment.IsProduction();

        options.TokenValidationParameters = new TokenValidationParameters
        {
            NameClaimType = "name",
            RoleClaimType = ClaimTypes.Role,
            ValidateAudience = false,
            ValidateLifetime = false
        };
        
        options.MetadataAddress = jwtSettings.MetadataAddress;
    });

builder.Services.AddAuthorization(options =>
{
    var defaultAuthorizationPolicyBuilder = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme, ApiKeyAuthenticationOptions.DefaultScheme);
    defaultAuthorizationPolicyBuilder = defaultAuthorizationPolicyBuilder
        .RequireAuthenticatedUser()
        .RequireRole("objectstore");
    options.DefaultPolicy = defaultAuthorizationPolicyBuilder.Build();
});

builder.Services.AddDbContextFactory<ApplicationDbContext>(options => { options.UseSqlite($"Data Source={Environment.GetEnvironmentVariable("DB_PATH")}"); });
builder.Services.AddScoped<IApiKeysService, ApiKeysService>();
//builder.Services.AddScoped<IAuthorizationFilter, ApiAuthorizationFilter>();
//builder.Services.AddScoped<IApiKeyValidator, ApiKeyValidator>();
builder.Services.AddScoped<StorageNameValidator>();
builder.Services.AddScoped<ISlug, StorageSlug>();
builder.Services.AddScoped<IAllowedHostsService, AllowedHostsService>();
builder.Services.AddScoped<IBucketsService, BucketsService>();
builder.Services.AddScoped<IStorageService<string>, StorageService>();
builder.Services.AddScoped<IKeyService, KeyService>();

builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o =>
{
    o.MapType<decimal>(() => new OpenApiSchema { Type = "number", Format = "decimal" });
    o.MapType<decimal?>(() => new OpenApiSchema { Type = "number", Format = "decimal", Nullable = true });
    o.CustomOperationIds(e => $"{e.ActionDescriptor.RouteValues["controller"]}{e.ActionDescriptor.RouteValues["action"]}");
    o.SwaggerDoc("v1", new OpenApiInfo { Version = "v1", Title = "SimpleObjectStore API" });
});

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
    ServeUnknownFileTypes = true,
    FileProvider = new PhysicalFileProvider(storageDirectory),
    RequestPath = "",
    OnPrepareResponse = ctx => StaticFileOptionsHandler.OnPrepareResponse(app, ctx)
});

app.Run();