using Application.Users.ConfirmEmail;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApi.Infrastructure;

namespace WebApi.Pages.Account;

public class ConfirmEmailModel(
    TokenHandler tokenHandler,
    ISender sender) : PageModel
{
    public string Message { get; set; }
    
    private CancellationToken CancellationToken => HttpContext.RequestAborted;

    public async Task<IActionResult> OnGetAsync(string userId, string token)
    {
        if (string.IsNullOrEmpty(userId) 
            || string.IsNullOrEmpty(token) 
            || !tokenHandler.ValidateEmailToken(token, out var tokenUserId)
            || userId != tokenUserId)
        {
            Message = "Invalid email confirmation link.";
            
            return Page();
        }

        var command = new ConfirmEmailCommand(userId);
        var result = await sender.Send(command, CancellationToken);
        
        Message = result.IsSuccess 
            ? "Thank you for confirming your email." 
            : "Error confirming your email.";

        return Page();
    }
}