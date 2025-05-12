using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApi.Pages.Account;

public class MfaSuccessModel : PageModel
{
    [TempData] 
    public List<string> RecoveryCodes { get; set; } = [];

    public IActionResult OnGet()
    {
        if (RecoveryCodes.Count == 0)
        {
            return RedirectToPage("Mfa");
        }
        
        return Page();
    }
}