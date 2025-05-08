using Application.Roles.GetById;
using Application.Roles.Remove;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApi.Pages.Roles;

public class DeleteModel(ISender sender) : PageModel
{
    public RoleDetails RoleInfo { get; set; } = new();
    
    private CancellationToken CancellationToken => HttpContext.RequestAborted;

    public class RoleDetails
    {
        public Guid Id { get; set; }
        public string RoleName { get; set; } = string.Empty;
    } 

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var query = new GetRoleByIdQuery(id);
        var cancellationToken = HttpContext.RequestAborted;
        var result = await sender.Send(query, cancellationToken);
        
        if (result.IsFailure) return Page();
        
        RoleInfo.RoleName = result.Value.Role.Name;
        TempData["Name"] = result.Value.Role.Name;
        
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid id)
    {
        var command = new RemoveRoleCommand(id);
        var result = await sender.Send(command, CancellationToken);

        if (!result.IsFailure) return LocalRedirect(Routes.Roles);
        
        ModelState.AddModelError(string.Empty, result.Error.Description);
        TempData.Keep("Name");
        
        if (TempData.TryGetValue("Name", out var value))
        {
            RoleInfo.RoleName = value?.ToString() ?? string.Empty;
        }
            
        return Page();
    }
}