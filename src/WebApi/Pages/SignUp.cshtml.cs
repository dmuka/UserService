using System.ComponentModel.DataAnnotations;
using Application.Users.SignIn;
using Application.Users.SignUp;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApi.Pages;

[AllowAnonymous]
public class SignUpModel(ISender sender, ILogger<SignUpModel> logger) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new ();

    public string ReturnUrl { get; set; } = string.Empty;

    public class InputModel
    {
        [Required]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 3)]
        [Display(Name = "User name")]
        public string UserName { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 3)]
        [Display(Name = "First name")]
        public string FirstName { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 3)]
        [Display(Name = "Last name")]
        public string LastName { get; set; }
        
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public void OnGet(string returnUrl = null)
    {
        ReturnUrl = returnUrl;
    }

    public async Task<IActionResult> OnPostAsync(string returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");

        if (!ModelState.IsValid) return Page();
        
        var signUpCommand = new SignUpUserCommand(
            Input.UserName, 
            Input.Email, 
            Input.FirstName, 
            Input.LastName, 
            Input.Password);
            
        var result = await sender.Send(signUpCommand);
        if (result.IsSuccess)
        {
            logger.LogInformation("User created a new account with password.");
                
            var signInCommand = new SignInUserCommand(Input.UserName, Input.Password);
            await sender.Send(signInCommand);
                
            return LocalRedirect(returnUrl);
        }
            
        ModelState.AddModelError(string.Empty, result.Error.Description);

        return Page();
    }
}