using System.ComponentModel.DataAnnotations;
using Application.Roles.Add;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApi.Infrastructure.PagesConstants;

namespace WebApi.Pages.Roles;

public class CreateModel(ISender sender) : PageModel
{
    [BindProperty] 
    public InputModel Input { get; set; } = new();
    
    public class InputModel
    {
        [Required]
        [StringLength(Lengths.MaxRoleName, ErrorMessage = ErrorMessages.RoleName, MinimumLength = Lengths.MinRoleName)]
        [Display(Name = "Role name")]
        public string RoleName { get; set; } = string.Empty;
    }

    public IActionResult OnGet()
    {
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var cancellationToken = HttpContext.RequestAborted;
        var command = new AddRoleCommand(Input.RoleName);
        var result = await sender.Send(command, cancellationToken);

        if (result.IsFailure) return Page();

        return LocalRedirect(Routes.Roles);
    }
}