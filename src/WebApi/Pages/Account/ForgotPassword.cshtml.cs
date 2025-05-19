using System.ComponentModel.DataAnnotations;
using Application.Abstractions.Email;
using Domain.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApi.Infrastructure;

namespace WebApi.Pages.Account;

public class ForgotPasswordModel(
    TokenHandler tokenHandler,
    IUserRepository userRepository,
    IEmailService emailService) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new ();
    
    private CancellationToken CancellationToken => HttpContext.RequestAborted;

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
            ModelState.AddModelError("NotFound", UserErrors.NotFoundByEmail(Input.Email).Description);
            
            return Page();
        }

        var resetCode = tokenHandler.GetEmailToken(user.Id.Value.ToString());
        var callbackUrl = Url.Page(
            Routes.ResetPassword,
            pageHandler: null,
            values: new { resetCode },
            protocol: Request.Scheme);
        
        await emailService.SendEmailAsync(
            Input.Email,
            "Reset password",
            $"Please reset your password by <a href='{callbackUrl}'>clicking here</a>.");

        return RedirectToPage(Routes.ForgotPasswordConfirmation);
    }
}