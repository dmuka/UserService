using System.Security.Claims;
using Application.Users.EnableMfa;
using Application.Users.GenerateQr;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApi.Pages.Account;

public class SetupMfaModel(ISender sender, ILogger<SetupMfaModel> logger) : PageModel
{
    [BindProperty]
    public string QrCode { get; set; } = string.Empty;

    [BindProperty]
    public string VerificationCode { get; set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync()
    {
        if (User.HasClaim("amr", "mfa")) return RedirectToPage("Mfa");

        var command = new GenerateQrCommand(User.Claims.First(claim => claim.Type == ClaimTypes.NameIdentifier).Value);
        var result = await sender.Send(command);
        
        if (result.IsSuccess)
        {
            logger.LogInformation("Qr code successfully generated.");

            QrCode = result.Value;
            
            return LocalRedirect(Routes.Mfa);
        }
        
        ModelState.AddModelError(string.Empty, result.Error.Description);;
        
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!int.TryParse(VerificationCode, out var code))
        {
            ModelState.AddModelError(string.Empty, "Invalid verification code.");
            
            return Page();
        }
        
        var command = new EnableMfaCommand(
            User.Claims.First(claim => claim.Type == ClaimTypes.NameIdentifier).Value,
            code);
        var result = await sender.Send(command);

        if (result.IsSuccess) return RedirectToPage("Mfa");
        
        ModelState.AddModelError(string.Empty, result.Error.Description);;
            
        return Page();
    }
}