using Application.Abstractions.Email;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApi.Pages;

[Authorize(Policy = "UserManagementPolicy")]
public class IndexModel(IHttpContextAccessor httpContextAccessor, IEmailService emailService) : PageModel
{
    public IActionResult OnGet()
    {
        emailService.SendEmailAsync("dmibel@gmail.com", "Test letter", "This is a test email.");
        
        return Page();
    }
}