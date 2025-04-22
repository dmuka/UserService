using Application.Users.GetAll;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApi.Pages.Users;

[Authorize(Policy = "UserManagementPolicy")]
public class IndexModel(ISender sender) : PageModel
{
    public PagedResult<UserResponse> PagedData { get; set; } = new();
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 2;

    [BindProperty(SupportsGet = true)]
    public string SearchString { get; set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync(int? pageNumber)
    {
        CurrentPage = pageNumber ?? 1;
        
        var query = new GetAllUsersQuery();
        var result = await sender.Send(query);
        
        if (result.IsFailure) return Page();
        
        var users = result.Value;

        if (!string.IsNullOrEmpty(SearchString))
        {
            users = users
                .Where(user =>
                    user.FirstName.Contains(SearchString) ||
                    user.LastName.Contains(SearchString) ||
                    user.Email.Contains(SearchString))
                .ToList();
        }
        
        var currentPageUsers = users
            .Skip((CurrentPage - 1) * PageSize)
            .Take(PageSize)
            .ToList();

        var pagesCount = (int)Math.Ceiling(users.Count / (double)PageSize);

        PagedData = new PagedResult<UserResponse>()
        {
            Items = currentPageUsers,
            PageNumber = CurrentPage,
            PageSize = PageSize,
            TotalItems = users.Count,
            TotalPages = pagesCount
        };

        return Page();
    }
}