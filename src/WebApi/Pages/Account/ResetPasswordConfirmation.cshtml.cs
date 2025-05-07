using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApi.Pages.Account;

public class ResetPasswordConfirmationModel : PageModel
{
    public IActionResult OnGet()
    {
        return Page();
    }
}