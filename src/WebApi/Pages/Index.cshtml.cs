using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApi.Infrastructure;

namespace WebApi.Pages;

public class IndexModel(IHttpContextAccessor httpContextAccessor, TokenHandler tokenHandler, IConfiguration _configuration) : PageModel
{
    public async Task<IActionResult> OnGet()
    {
        var principal = httpContextAccessor.HttpContext.User;
        
        if (!principal.Identity.IsAuthenticated) return RedirectToPage("/SignIn"); 
        
        return Page();
    }
}