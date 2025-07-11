﻿using System.ComponentModel.DataAnnotations;
using Application.Users.SignUp;
using Domain.Roles;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebApi.Infrastructure.PagesConstants;

namespace WebApi.Pages.Users;

public class CreateModel(ISender sender, IRoleRepository roleRepository) : PageModel
{
    [BindProperty] 
    public InputModel Input { get; set; } = new();

    public List<SelectListItem> AllRoles { get; set; } = [];
    
    public class InputModel
    {
        [Required]
        [StringLength(Lengths.MaxUserName, ErrorMessage = ErrorMessages.UserName, MinimumLength = Lengths.MinUserName)]
        [Display(Name = "User name")]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [StringLength(Lengths.MaxFirstName, ErrorMessage = ErrorMessages.UserFirstName, MinimumLength = Lengths.MinFirstName)]
        [Display(Name = "First name")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(Lengths.MaxLastName, ErrorMessage = ErrorMessages.UserLastName, MinimumLength = Lengths.MinLastName)]
        [Display(Name = "Last name")]
        public string LastName { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        [Display(Name = "Is MFA enabled")]
        public bool IsMfaEnabled { get; set; }
        
        public string MfaSecret { get; set; } = string.Empty;

        [Required]
        [StringLength(Lengths.MaxPassword, ErrorMessage = ErrorMessages.Password, MinimumLength = Lengths.MinPassword)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = ErrorMessages.ConfirmPassword)]
        public string ConfirmPassword { get; set; } = string.Empty;
        
        [Display(Name = "User role(s)")]
        public List<string> SelectedRoles { get; set; } = [];
    }
    
    public async Task<IActionResult> OnGet()
    {
        AllRoles = (await roleRepository.GetAllRolesAsync())
            .Select(role => new SelectListItem(role.Name, role.Id.Value.ToString())).ToList();
        
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var cancellationToken = HttpContext.RequestAborted;
        var command = new SignUpUserCommand(
            Input.UserName,
            Input.Email,
            Input.FirstName,
            Input.LastName,
            Input.Password,
            Input.IsMfaEnabled,
            Input.MfaSecret,
            Input.SelectedRoles);
        var result = await sender.Send(command, cancellationToken);

        if (result.IsFailure) return Page();

        return LocalRedirect(Routes.Users);
    }
}