using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SimpleObjectStore.Admin.Pages;

public class Logout : PageModel
{
    public IActionResult OnGetAsync()
    {
        return SignOut("Cookies", "oidc");
    }
}