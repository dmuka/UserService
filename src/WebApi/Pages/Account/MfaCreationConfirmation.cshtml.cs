using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApi.Pages.Account;

public class MfaSuccessModel : PageModel
{
    [TempData] 
    public string[] RecoveryCodes { get; set; } = [];

    public IActionResult OnGet()
    {
        if (TempData.TryGetValue("RecoveryCodes", out var rc))
        {
            if (rc is string[] recoveryCodesArray)
            {
                RecoveryCodes = recoveryCodesArray;
            }
            else
            {
                RecoveryCodes = [];
            }            
            TempData.Keep("RecoveryCodes");
        }
        
        if (RecoveryCodes.Length == 0)
        {
            return RedirectToPage("Mfa");
        }
        
        return Page();
    }
}