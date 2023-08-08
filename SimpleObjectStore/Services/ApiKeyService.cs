using System.Security.Cryptography;
using SimpleObjectStore.Services.Interfaces;

namespace SimpleObjectStore.Services;

/// <summary>
/// Source: https://www.camiloterevinto.com/post/simple-and-secure-api-keys-using-asp-net-core
/// </summary>
public class ApiKeyService : IApiKeyService
{
    private const string _prefix = "";
    private const int _numberOfSecureBytesToGenerate = 32;
    private const int _lengthOfKey = 32;

    public string GenerateApiKey()
    {
        var bytes = RandomNumberGenerator.GetBytes(_numberOfSecureBytesToGenerate);

        string base64String = Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_");

        var keyLength = _lengthOfKey - _prefix.Length;

        return _prefix + base64String[..keyLength];
    }
}