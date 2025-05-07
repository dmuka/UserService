using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApi.Infrastructure;

namespace WebApi.Pages.Account;

[AllowAnonymous]
public class LogoutModel(TokenHandler tokenHandler, ILogger<LogoutModel> logger) : PageModel
{
    public IActionResult OnGet() => Page();

    public IActionResult OnPost(string? returnUrl = null)
    {
        tokenHandler.ClearTokens();
        logger.LogInformation("User logged out.");
        
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }

        return LocalRedirect(Routes.SignIn);
    }
}