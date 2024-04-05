using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;

namespace SimpleObjectStore.Admin.Controller;

[Route("/[action]")]
public class AuthController : Microsoft.AspNetCore.Mvc.Controller
{
    public async Task Login()
    {
        await HttpContext.ChallengeAsync(new AuthenticationProperties { RedirectUri = "/" });
    }

    public async Task Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
    }
}