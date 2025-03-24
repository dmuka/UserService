using System.ComponentModel.DataAnnotations;
using Application.Users.SignIn;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApi.Pages;

public class SignInModel(ISender sender, ILogger<SignInModel> logger) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new ();

    public string ReturnUrl { get; set; } = string.Empty;

    [TempData]
    public string ErrorMessage { get; set; } = string.Empty;

    public class InputModel
    {
        [Required]
        [MinLength(5)]
        public string UserName { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
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
            Input.Email);

        var result = await sender.Send(command);
        if (result.IsSuccess)
        {
            logger.LogInformation("User logged in.");
            return LocalRedirect(returnUrl);
        }
        // if (result.RequiresTwoFactor)
        // {
        //     return RedirectToPage("./SignInWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
        // }
        // if (result.IsLockedOut)
        // {
        //     _logger.LogWarning("User account locked out.");
        //     return RedirectToPage("./Lockout");
        // }
        
        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        return Page();
    }
}