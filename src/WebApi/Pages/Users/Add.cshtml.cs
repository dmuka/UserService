using System.ComponentModel.DataAnnotations;
using Application.Users.SignUp;
using Domain.Roles;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApi.Pages.Users;

public class CreateModel(ISender sender, IRoleRepository roleRepository) : PageModel
{
    [BindProperty] 
    public InputModel Input { get; set; } = new();

    public List<SelectListItem> AllRoles { get; set; } = [];
    
    public class InputModel
    {
        [Required]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 3)]
        [Display(Name = "User name")]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 3)]
        [Display(Name = "First name")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 3)]
        [Display(Name = "Last name")]
        public string LastName { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
        
        [Display(Name = "User role(s)")]
        public List<string> SelectedRoles { get; set; } = [];
    }
    
    public async Task<IActionResult> OnGet()
    {
        AllRoles = (await roleRepository.GetAllRolesAsync())
            .Select(role => new SelectListItem(role.Name, role.Id.ToString())).ToList();
        
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var command = new SignUpUserCommand(
            Input.UserName, 
            Input.Email, 
            Input.FirstName, 
            Input.LastName, 
            Input.Password,
            Input.SelectedRoles.Select(Guid.Parse).ToList());
        var result = await sender.Send(command);

        if (result.IsFailure) return Page();

        return RedirectToPage("Users/Index");
    }
}