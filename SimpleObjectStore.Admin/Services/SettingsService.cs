namespace SimpleObjectStore.Admin.Services;

public interface ISettingsService
{
    string? GetEndpoint();
    string? GetKey();
    string? GetOpenIdAuthority();
    string? GetOpenIdClientId();
    string? GetOpenIdClientSecret();
}

public class SettingsService : ISettingsService
{
    private readonly IConfiguration _configuration;

    public SettingsService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string? GetEndpoint() => _configuration["API:Endpoint"] ?? Environment.GetEnvironmentVariable("API_ENDPOINT");

    public string? GetKey() => _configuration["API:Key"] ?? Environment.GetEnvironmentVariable("API_KEY");

    public string? GetOpenIdAuthority() => _configuration["OpenId:Authority"] ?? Environment.GetEnvironmentVariable("OPENID_AUTHORITY");

    public string? GetOpenIdClientId() => _configuration["OpenId:ClientId"] ?? Environment.GetEnvironmentVariable("OPENID_CLIENT_ID");

    public string? GetOpenIdClientSecret() => _configuration["OpenId:ClientSecret"] ?? Environment.GetEnvironmentVariable("OPENID_CLIENT_SECRET");
}