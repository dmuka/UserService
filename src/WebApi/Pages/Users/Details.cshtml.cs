using Application.Users.GetById;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApi.Pages.Users;

public class DetailsModel(ISender sender) : PageModel
{
    public UserDetails UserInfo { get; set; } = new();

    public List<SelectListItem> UserRoles { get; set; } = [];

    public class UserDetails
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string[] Roles { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var query = new GetUserByIdQuery(id);
        var result = await sender.Send(query);
        
        if (result.IsFailure) return Page();
        
        UserInfo.Username = result.Value.Username;
        UserInfo.FirstName = result.Value.FirstName;
        UserInfo.LastName = result.Value.LastName;
        UserInfo.Email = result.Value.Email;
        UserInfo.Roles = result.Value.Roles.Select(role => role.name).ToArray();
        
        UserRoles = UserInfo.Roles
            .Select(role => new SelectListItem(role, string.Empty)).ToList();
        
        return Page();
    }
}