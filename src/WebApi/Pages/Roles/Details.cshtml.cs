using Application.Roles.GetById;
using Application.Users.GetById;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApi.Pages.Roles;

public class DetailsModel(ISender sender) : PageModel
{
    public RoleDetails RoleInfo { get; set; } = new();

    public List<SelectListItem> UserRoles { get; set; } = [];

    public class RoleDetails
    {
        public Guid Id { get; set; }
        public string RoleName { get; set; } = string.Empty;
    } 

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var query = new GetRoleByIdQuery(id);
        var result = await sender.Send(query);
        
        if (result.IsFailure) return Page();
        
        RoleInfo.RoleName = result.Value.Role.Name;
        
        return Page();
    }
}