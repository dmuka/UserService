using Application.Abstractions.Authentication;
using Application.Users.EnableMfa;
using Application.Users.GenerateMfaArtifacts;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApi.Pages.Account;

public class SetupMfaModel(
    ISender sender, 
    IUserContext userContext, 
    ILogger<SetupMfaModel> logger) : PageModel
{
    [TempData]
    public string QrCode { get; set; } = string.Empty;
    
    [BindProperty]
    public string VerificationCode { get; set; } = string.Empty;
    
    [TempData]
    public IList<string> RecoveryCodes { get; set; } = [];

    public async Task<IActionResult> OnGetAsync()
    {
        if (userContext.AuthMethod == "mfa") return RedirectToPage("Mfa");

        var command = new GenerateMfaArtifactsCommand(userContext.UserId.ToString());
        var result = await sender.Send(command);
        
        if (result.IsSuccess)
        {
            logger.LogInformation("Qr code and recovery codes generated successfully.");
            QrCode = result.Value.qr;
            RecoveryCodes = result.Value.codes;
            
            return Page();
        }
        
        ModelState.AddModelError(string.Empty, result.Error.Description);
        
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        
        if (TempData.TryGetValue("Qr", out var qr))
        {
            QrCode = qr?.ToString() ?? string.Empty;            
            TempData.Keep("Qr");
        }
        
        if (!int.TryParse(VerificationCode, out var code))
        {
            ModelState.AddModelError("Verification code", "Invalid verification code.");
            
            return Page();
        }
        
        var command = new EnableMfaCommand(userContext.UserId.ToString(), code);
        var result = await sender.Send(command);

        if (result.IsSuccess)
        {
            return RedirectToPage("Mfa");
        }

        ModelState.AddModelError("MFA error", result.Error.Description);
        
        return Page();
    }
}