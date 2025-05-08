using System.ComponentModel.DataAnnotations;
using Application.Roles.GetById;
using Application.Roles.Update;
using Domain.Roles;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApi.Infrastructure.PagesConstants;

namespace WebApi.Pages.Roles;

public class EditModel(ISender sender) : PageModel
{
    [BindProperty]
    public InputModel RoleInfo { get; set; } = new();
    
    private CancellationToken CancellationToken => HttpContext.RequestAborted;

    public class InputModel
    {
        public Guid Id { get; set; }
        
        [Required]
        [StringLength(Lengths.MaxRoleName, ErrorMessage = ErrorMessages.RoleName, MinimumLength = Lengths.MinRoleName)]
        [Display(Name = "Role name")]
        public string RoleName { get; set; } = string.Empty;
    }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var query = new GetRoleByIdQuery(id);
        var result = await sender.Send(query, CancellationToken);

        if (result.IsFailure)
        {
            ModelState.AddModelError(string.Empty, result.Error.Description);
            
            return Page();
        }
        
        RoleInfo.RoleName = result.Value.Role.Name;
        TempData["Id"] = result.Value.Role.Id.Value;
        
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        if (Guid.TryParse(TempData["Id"]?.ToString(), out var id))
        {
            RoleInfo.Id = id;
        }
        else
        {
            return Page();
        }
        
        var role = Role.Create(RoleInfo.Id, RoleInfo.RoleName).Value;
        
        var command = new UpdateRoleCommand(role);
        var result = await sender.Send(command, CancellationToken);
        
        return result.IsFailure ? Page() : LocalRedirect(Routes.Roles);
    }
}