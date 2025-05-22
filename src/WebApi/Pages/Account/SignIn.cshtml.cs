using System.ComponentModel.DataAnnotations;
using Application.Users.SignIn;
using Infrastructure.Options.Authentication;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using WebApi.Infrastructure;
using WebApi.Infrastructure.PagesConstants;

namespace WebApi.Pages.Account;

public class SignInModel(
    ISender sender, 
    TokenHandler tokenHandler,
    IOptions<AuthOptions> authOptions,
    ILogger<SignInModel> logger) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new ();
    
    public bool ShowRecoveryCodeOption { get; set; }

    public string? ReturnUrl { get; set; } = string.Empty;

    [TempData]
    public string ErrorMessage { get; set; } = string.Empty;
    
    private CancellationToken CancellationToken => HttpContext.RequestAborted;

    public class InputModel
    {
        [Required]
        [StringLength(Lengths.MaxUserName, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.",
            MinimumLength = Lengths.MinUserName)]
        public string UserName { get; set; } = string.Empty;

        [EmailAddress]
        [Display(Name = "Email")]
        public string? Email { get; set; } = string.Empty;

        [Required]
        [StringLength(Lengths.MaxPassword, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = Lengths.MinPassword)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
        
        public string VerificationCode { get; set; } = string.Empty;
        
        [Display(Name = "Or enter recovery code")]
        public string? RecoveryCode { get; set; } = string.Empty;
    }

    public void OnGet(string? returnUrl = null)
    {
        if (!string.IsNullOrEmpty(ErrorMessage))
        {
            ModelState.AddModelError(string.Empty, ErrorMessage);
        }

        returnUrl ??= Url.Content("~/");

        ReturnUrl = returnUrl;
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");

        if (!ModelState.IsValid) return Page();

        var command = new SignInUserCommand(
            Input.UserName,
            Input.Password,
            Input.RememberMe,
            authOptions.Value.RefreshTokenExpirationInDays,
            Input.VerificationCode,
            Input.RecoveryCode,
            Input.Email);

        var result = await sender.Send(command, CancellationToken);
        if (result.IsFailure)
        {
            ModelState.AddModelError(string.Empty, $"Invalid login attempt ({result.Error.Description}).");

            ShowRecoveryCodeOption = true;
            tokenHandler.ClearTokens();
            logger.LogInformation("User logged out.");
            
            return Page();
        }

        logger.LogInformation("User logged in.");
        tokenHandler.StoreToken(result.Value.AccessToken);
        tokenHandler.StoreSessionId(result.Value.SessionId);

        return LocalRedirect(returnUrl);
    }
}