using System.ComponentModel.DataAnnotations;
using System.Web;
using Application.Abstractions.Email;
using Application.Users.SignIn;
using Application.Users.SignUp;
using Infrastructure.Options.Authentication;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using WebApi.Infrastructure;
using WebApi.Infrastructure.PagesConstants;

namespace WebApi.Pages.Account;

[AllowAnonymous]
public class SignUpModel(
    TokenHandler tokenHandler,
    IOptions<AuthOptions> authOptions,
    ISender sender,
    IEmailService emailService,
    ILogger<SignUpModel> logger) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new ();

    public string? ReturnUrl { get; set; }
    
    private CancellationToken CancellationToken => HttpContext.RequestAborted;

    public class InputModel
    {
        [Required]
        [StringLength(Lengths.MaxUserName, ErrorMessage = ErrorMessages.UserName, MinimumLength = Lengths.MinUserName)]
        [Display(Name = "User name")]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [StringLength(Lengths.MaxFirstName, ErrorMessage = ErrorMessages.UserFirstName, MinimumLength = Lengths.MinFirstName)]
        [Display(Name = "First name")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(Lengths.MaxLastName, ErrorMessage = ErrorMessages.UserLastName, MinimumLength = Lengths.MinLastName)]
        [Display(Name = "Last name")]
        public string LastName { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(Lengths.MaxPassword, ErrorMessage = ErrorMessages.Password, MinimumLength = Lengths.MinPassword)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = ErrorMessages.ConfirmPassword)]
        public string ConfirmPassword { get; set; } = string.Empty;
        
        public bool IsMfaEnabled { get; set; } = false;

        public string? MfaSecret { get; set; } = null;
    }

    public void OnGet(string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");
        
        ReturnUrl = returnUrl;
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");

        if (!ModelState.IsValid) return Page();
        
        var signUpCommand = new SignUpUserCommand(
            Input.UserName, 
            Input.Email, 
            Input.FirstName, 
            Input.LastName, 
            Input.Password,
            Input.IsMfaEnabled,
            Input.MfaSecret);
        var result = await sender.Send(signUpCommand, CancellationToken);
        
        if (result.IsSuccess)
        {
            logger.LogInformation("User with id: {Id} created a new account with password.", result.Value);
           
            var signInCommand = new SignInUserCommand(
                Input.UserName, 
                Input.Password, 
                false,
                authOptions.Value.RefreshTokenExpirationInDays);
            await sender.Send(signInCommand, CancellationToken);
                
            return LocalRedirect(returnUrl);
        }
            
        ModelState.AddModelError(string.Empty, result.Error.Description);

        return Page();
    }
}