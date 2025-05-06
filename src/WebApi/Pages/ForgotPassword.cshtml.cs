using System.ComponentModel.DataAnnotations;
using Application.Abstractions.Email;
using Application.Users.SignIn;
using Domain.Users;
using Infrastructure.Options.Authentication;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using WebApi.Infrastructure;

namespace WebApi.Pages;

public class ForgotPasswordModel(
    TokenHandler tokenHandler,
    IUserRepository userRepository,
    IEmailService emailService,
    ILogger<ForgotPasswordModel> logger) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new ();
    
    private CancellationToken CancellationToken => HttpContext.RequestAborted;

    [TempData]
    public string ErrorMessage { get; set; } = string.Empty;

    public class InputModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;
    }

    public IActionResult OnGet()
    {
        return Page();
    }
    
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        
        var user = await userRepository.GetUserByEmailAsync(Input.Email, CancellationToken);
        if (user is null)
        {
            ModelState.AddModelError(string.Empty, UserErrors.NotFoundByEmail(Input.Email).Description);
            
            return RedirectToPage("./ForgotPasswordConfirmation");
        }

        var resetCode = tokenHandler.GetPasswordResetToken(user.Id.Value.ToString());
        var callbackUrl = Url.Page(
            "/ResetPassword",
            pageHandler: null,
            values: new { resetCode },
            protocol: Request.Scheme);
        
        await emailService.SendEmailAsync(
            Input.Email,
            "Reset Password",
            $"Please reset your password by <a href='{callbackUrl}'>clicking here</a>.");

        return RedirectToPage("./ForgotPasswordConfirmation");
    }
}