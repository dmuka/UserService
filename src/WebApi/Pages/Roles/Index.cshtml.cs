using Application.Roles.GetAll;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApi.Pages.Roles;

[Authorize(Policy = "UserManagementPolicy")]
public class IndexModel(ISender sender) : PageModel
{
    public PagedResult<RoleResponse> PagedData { get; set; } = new();
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int VisiblePagesRange { get; set; } = 3;

    [BindProperty(SupportsGet = true)]
    public string SearchString { get; set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync(int? pageNumber, int? pageSize)
    {
        CurrentPage = pageNumber ?? 1;
        PageSize = pageSize ?? 10;
        
        var query = new GetAllRolesQuery();
        var result = await sender.Send(query);
        
        if (result.IsFailure) return Page();
        
        var roles = result.Value;

        if (!string.IsNullOrEmpty(SearchString))
        {
            roles = roles
                .Where(role => role.Name.Contains(SearchString))
                .ToList();
        }
        
        var currentPageRoles = roles
            .Skip((CurrentPage - 1) * PageSize)
            .Take(PageSize)
            .ToList();

        var pagesCount = (int)Math.Ceiling(roles.Count / (double)PageSize);
        var firstVisiblePage = Math.Max(1, CurrentPage - VisiblePagesRange / 2);
        var lastVisiblePage = Math.Min(pagesCount, firstVisiblePage + VisiblePagesRange - 1);

        PagedData = new PagedResult<RoleResponse>
        {
            Items = currentPageRoles,
            PageNumber = CurrentPage,
            PageSize = PageSize,
            TotalItems = roles.Count,
            TotalPages = pagesCount,
            VisiblePagesRange = VisiblePagesRange,
            FirstVisiblePage = firstVisiblePage,
            LastVisiblePage = lastVisiblePage
        };

        return Page();
    }
}