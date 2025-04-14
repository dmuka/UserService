using System.ComponentModel.DataAnnotations;
using Application.Users.SignIn;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApi.Infrastructure;
using WebApi.Infrastructure.PagesConstants;

namespace WebApi.Pages;

public class SignInModel(
    ISender sender, 
    TokenHandler tokenHandler,
    IHttpContextAccessor contextAccessor, 
    ILogger<SignInModel> logger) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new ();

    public string ReturnUrl { get; set; } = string.Empty;

    [TempData]
    public string ErrorMessage { get; set; } = string.Empty;

    public class InputModel
    {
        [Required]
        [StringLength(Lengths.MaxUserName, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.",
            MinimumLength = Lengths.MinUserName)]
        public string UserName { get; set; } = string.Empty;

        [EmailAddress]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Required]
        [StringLength(Lengths.MaxPassword, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = Lengths.MinPassword)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

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
            tokenHandler.StoreTokens(result.Value.AccessToken, result.Value.RefreshToken); 

            return LocalRedirect(returnUrl);
        }
        
        ModelState.AddModelError(string.Empty, "Invalid login attempt");
        return Page();
    }
}