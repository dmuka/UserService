using Application.Users.GetAll;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApi.Pages.Users;

[Authorize(Policy = "UserManagementPolicy")]
public class IndexModel(ISender sender) : PageModel
{
    public IList<UserResponse> Users { get; set; } = [];

    [BindProperty(SupportsGet = true)]
    public string SearchString { get; set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync()
    {
        var query = new GetAllUsersQuery();
        var result = await sender.Send(query);
        
        if (result.IsFailure) return Page();
        
        var users = result.Value;

        if (!string.IsNullOrEmpty(SearchString))
        {
            users = users.Where(user => 
                user.FirstName.Contains(SearchString) || 
                user.LastName.Contains(SearchString) ||
                user.Email.Contains(SearchString));
        }
        
        Users = users.ToList();

        return Page();
    }
}