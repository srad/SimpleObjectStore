using Microsoft.AspNetCore.Mvc;

namespace SimpleObjectStore.Filters;

public class ApiKeyAttribute : ServiceFilterAttribute
{
    public ApiKeyAttribute()
        : base(typeof(ApiKeyAuthorizationFilter))
    {
    }
}