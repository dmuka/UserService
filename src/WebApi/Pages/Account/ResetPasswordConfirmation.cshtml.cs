using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApi.Infrastructure;

namespace WebApi.Pages.Account;

public class ResetPasswordConfirmationModel(TokenHandler tokenHandler) : PageModel
{
    public IActionResult OnGet()
    {
        return Page();
    }
}