using Application.Abstractions.Authentication;
using Application.Abstractions.Email;
using Domain.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApi.Pages;

//[Authorize(Policy = "UserManagementPolicy")]
public class IndexModel(IUserRepository userRepository, IUserContext userContext) : PageModel
{
    public bool IsEmailConfirmed { get; set; }
    
    private CancellationToken CancellationToken => HttpContext.RequestAborted;

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await userRepository.GetUserByIdAsync(userContext.UserId, CancellationToken);
        if (user is not null)
        {
            IsEmailConfirmed = user.IsEmailConfirmed;
        }
        
        return Page();
    }
}