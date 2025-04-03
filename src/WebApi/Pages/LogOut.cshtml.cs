using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApi.Infrastructure;

namespace WebApi.Pages;

[AllowAnonymous]
public class LogoutModel(TokenHandler tokenHandler, ILogger<LogoutModel> logger) : PageModel
{
    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPost(string returnUrl = null)
    {
        tokenHandler.ClearTokens();
        logger.LogInformation("User logged out.");
        
        if (returnUrl != null)
        {
            return LocalRedirect(returnUrl);
        }

        return RedirectToPage();
    }
}