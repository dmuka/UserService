using Application.Users.GetAll;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApi.Pages.Users;

[Authorize(Policy = "UserManagementPolicy")]
public class IndexModel(ISender sender) : PageModel
{
    public PagedResult<UserResponse> PagedData { get; set; }
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 2;
    public IList<UserResponse> Users { get; set; } = [];

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
                    user.Email.Contains(SearchString));
        }

        var userResponses = users as UserResponse[] ?? users.ToArray();
        Users = userResponses
            .Skip((CurrentPage - 1) * PageSize)
            .Take(PageSize)
            .ToList();

        PagedData = new PagedResult<UserResponse>()
        {
            Items = Users,
            PageNumber = CurrentPage,
            PageSize = PageSize,
            TotalItems = userResponses.Length
        };

        return Page();
    }
}