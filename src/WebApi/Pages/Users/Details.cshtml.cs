using Application.Users.GetById;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApi.Pages.Users;

public class DetailsModel(ISender sender) : PageModel
{
    public InputModel Input { get; set; } = new();

    public List<SelectListItem> UserRoles { get; set; } = [];

    public class InputModel
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string IsMfaEnabled { get; set; } = string.Empty;
        public string[] Roles { get; set; } = [];
    } 

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var query = new GetUserByIdQuery(id);
        var result = await sender.Send(query);
        
        if (result.IsFailure) return Page();
        
        Input.Username = result.Value.Username;
        Input.FirstName = result.Value.FirstName;
        Input.LastName = result.Value.LastName;
        Input.Email = result.Value.Email;
        Input.IsMfaEnabled = result.Value.IsMfaEnabled;
        Input.Roles = result.Value.Roles.Select(role => role.name).ToArray();
        
        UserRoles = Input.Roles
            .Select(role => new SelectListItem(role, string.Empty)).ToList();
        
        return Page();
    }
}