using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SimpleObjectStore.Filters;

public class ApiKeyAttribute : ServiceFilterAttribute
{
    public ApiKeyAttribute() : base(typeof(IAuthorizationFilter))
    {
    }
}