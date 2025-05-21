using System.Web;
using Application.Abstractions.Authentication;
using Application.Abstractions.Email;
using Domain.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApi.Infrastructure;

namespace WebApi.Pages.Account;

public class ResendConfirmationEmailModel(
    IUserContext userContext,
    IUserRepository userRepository,
    TokenHandler tokenHandler,
    IEmailService emailService) : PageModel
{
    public string Message { get; set; } = string.Empty;

    public async Task<IActionResult> OnPostAsync()
    {
        var userId = userContext.UserId;
        var user = await userRepository.GetUserByIdAsync(userId, HttpContext.RequestAborted);
        if (user == null)
        {
            return RedirectToPage(Routes.SignIn);
        }

        if (user.IsEmailConfirmed)
        {
            Message = "Your email is already confirmed.";
            
            return Page();
        }

        var token = tokenHandler.GetEmailToken(userId.ToString());
        var confirmationLink = Url.Page(
            Routes.ConfirmEmail,
            pageHandler: null,
            values: new { userId, token = HttpUtility.UrlEncode(token) },
            protocol: Request.Scheme);
            
        var emailBody = $"<p>Please confirm your email by clicking <a href='{confirmationLink}'>here</a>.</p>";
            
        await emailService.SendEmailAsync(user.Email, "Confirm your email", emailBody);

        Message = "A new confirmation email has been sent.";
        
        return Page();
    }
}