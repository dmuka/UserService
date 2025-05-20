using Application.Abstractions.Authentication;
using Application.Users.GenerateMfaArtifacts;
using Domain.Users;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApi.Pages.Account;

public class MfaModel(
    IUserContext userContext, 
    IUserRepository userRepository,
    ISender sender) : PageModel
{
    public bool IsMfaEnabled { get; set; }
    public int RecoveryCodesCount { get; set; }
    
    private CancellationToken CancellationToken => HttpContext.RequestAborted;

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await userRepository.GetUserByIdAsync(userContext.UserId, CancellationToken);
        if (user is null) return NotFound();

        IsMfaEnabled = user.IsMfaEnabled;
        RecoveryCodesCount = user.IsMfaEnabled
             ? user.RecoveryCodesHashes?.Count ?? 0
             : 0;
        
        return Page();
    }

    public async Task<IActionResult> OnPostDisableAsync()
    {
        var user = await userRepository.GetUserByIdAsync(userContext.UserId, CancellationToken);
        if (user is null) return NotFound();
        
        user.DisableMfa();
        await userRepository.UpdateUserAsync(user, CancellationToken);
        
        return Page();
    }

    public async Task<IActionResult> OnPostRegenerateCodesAsync()
    {
        var user = await userRepository.GetUserByIdAsync(userContext.UserId, CancellationToken);
        if (user is null) return NotFound();

        var command = new GenerateMfaArtifactsCommand(userContext.UserId.ToString());
        var result = await sender.Send(command, CancellationToken);

        if (result.IsSuccess)
        {
            var recoveryCodes = result.Value.codes.ToArray();
            TempData["RecoveryCodes"] = recoveryCodes;

            return RedirectToPage(Routes.MfaCreationConfirmation);
        }
        
        ModelState.AddModelError(string.Empty, result.Error.Description);
        
        return Page();
    }
}