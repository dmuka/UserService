using System.ComponentModel.DataAnnotations;
using Application.Abstractions.Authentication;
using Application.Users.GetById;
using Application.Users.Update;
using Domain.Roles;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebApi.Infrastructure.PagesConstants;

namespace WebApi.Pages.Users;

public class EditModel(
    ISender sender, 
    IPasswordHasher passwordHasher, 
    IRoleRepository roleRepository) : PageModel
{
    [BindProperty]
    public InputModel UserInfo { get; set; } = new();

    public List<SelectListItem> UserRoles { get; set; } = [];

    public class InputModel
    {
        internal Guid Id { get; set; }
        
        [Required]
        [StringLength(50, ErrorMessage = ErrorMessages.UserName, MinimumLength = 3)]
        [Display(Name = "User name")]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [StringLength(100, ErrorMessage = ErrorMessages.UserFirstName, MinimumLength = 3)]
        [Display(Name = "First name")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100, ErrorMessage = ErrorMessages.UserLastName, MinimumLength = 3)]
        [Display(Name = "Last name")]
        public string LastName { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100, ErrorMessage = ErrorMessages.Password, MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Old password")]
        public string OldPassword { get; set; } = string.Empty;

        [Required]
        [StringLength(100, ErrorMessage = ErrorMessages.Password, MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("Password", ErrorMessage = ErrorMessages.ConfirmPassword)]
        public string ConfirmNewPassword { get; set; } = string.Empty;
        
        [Display(Name = "User role(s)")]
        public List<string> SelectedRoles { get; set; } = [];
        public List<RoleId> SelectedRolesIds { get; set; } = [];
    }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var query = new GetUserByIdQuery(id);
        var result = await sender.Send(query);
        
        if (result.IsFailure) return Page();
        
        UserInfo.SelectedRoles = result.Value.Roles.Select(role => role.name).ToList();
        UserInfo.SelectedRolesIds = result.Value.Roles.Select(role => new RoleId(role.id)).ToList();
        
        return Page();
    }
    
    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid) return Page();

        var user = Domain.Users.User.Create(
            UserInfo.Id,
            UserInfo.UserName,
            UserInfo.FirstName,
            UserInfo.LastName,
            passwordHasher.GetHash(UserInfo.NewPassword),
            UserInfo.Email,
            UserInfo.SelectedRolesIds,
            []).Value;
        
        var command = new UpdateUserCommand(user);
        var result = await sender.Send(command, cancellationToken);
        
        return result.IsFailure ? Page() : LocalRedirect("/Users");
    }
}