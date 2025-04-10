using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApi.Pages;

[Authorize(Policy = "UserManagementPolicy")]
public class IndexModel(IHttpContextAccessor httpContextAccessor) : PageModel
{
    public IActionResult OnGet()
    {
        if (httpContextAccessor.HttpContext is null) return RedirectToPage(Routes.SignIn); 
        
        var principal = httpContextAccessor.HttpContext.User;
        
        if (principal.Identity is null || !principal.Identity.IsAuthenticated) 
            return RedirectToPage(Routes.SignIn); 
        
        return Page();
    }
}