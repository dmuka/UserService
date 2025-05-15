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
    public int PageSize { get; set; } = 10;
    public int VisiblePagesRange { get; set; } = 3;
    
    [BindProperty(SupportsGet = true)]
    public string? SearchString { get; set; }

    public async Task<IActionResult> OnGetAsync(int? pageNumber, int? pageSize)
    {
        CurrentPage = pageNumber ?? 1;
        PageSize = pageSize ?? 10;
        
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
        var firstVisiblePage = Math.Max(1, CurrentPage - VisiblePagesRange / 2);
        var lastVisiblePage = Math.Min(pagesCount, firstVisiblePage + VisiblePagesRange - 1);

        PagedData = new PagedResult<UserResponse>()
        {
            Items = currentPageUsers,
            PageNumber = CurrentPage,
            PageSize = PageSize,
            TotalItems = users.Count,
            TotalPages = pagesCount,
            VisiblePagesRange = VisiblePagesRange,
            FirstVisiblePage = firstVisiblePage,
            LastVisiblePage = lastVisiblePage
        };

        return Page();
    }
}