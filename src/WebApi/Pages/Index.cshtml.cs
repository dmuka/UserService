using Application.Abstractions.Email;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApi.Pages;

[Authorize(Policy = "UserManagementPolicy")]
public class IndexModel : PageModel
{
    public IActionResult OnGet()
    {
        return Page();
    }
}