using System.ComponentModel.DataAnnotations;
using Application.Users.ResetPassword;
using Domain.Users;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApi.Infrastructure;
using WebApi.Infrastructure.PagesConstants;

namespace WebApi.Pages.Account;

public class ResetPasswordModel(
    TokenHandler tokenHandler, 
    ISender sender,
    ILogger<ResetPasswordConfirmationModel> logger) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();
    
    private CancellationToken CancellationToken => HttpContext.RequestAborted;

    public class InputModel
    {
        [Required]
        [StringLength(Lengths.MaxPassword, ErrorMessage = ErrorMessages.Password, MinimumLength = Lengths.MinPassword)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = ErrorMessages.ConfirmPassword)]
        public string ConfirmPassword { get; set; } = string.Empty;

        public string Code { get; set; } = string.Empty;
    }

    public IActionResult OnGet(string? resetCode = null)
    {
        if (resetCode is null || !tokenHandler.ValidatePasswordResetToken(resetCode, out _))
        {
            ModelState.AddModelError("Code", "Valid reset code is required.");

            return Page();
        }

        Input = new InputModel { Code = resetCode };

        tokenHandler.ClearTokens();
        logger.LogInformation("User logged out.");
        
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        if (!tokenHandler.ValidatePasswordResetToken(Input.Code, out var userId)
            || !Guid.TryParse(userId, out var userGuid))
        {
            ModelState.AddModelError(string.Empty, UserErrors.WrongResetCode().Description);
            
            return Page();
        }

        var command = new ResetPasswordCommand(userGuid, Input.ConfirmPassword);
        var result = await sender.Send(command, CancellationToken);

        if (result.IsSuccess)
        {
            return RedirectToPage(Routes.ResetPasswordConfirmation);
        }

        ModelState.AddModelError(string.Empty, result.Error.Description);
            
        return Page();
    }
}