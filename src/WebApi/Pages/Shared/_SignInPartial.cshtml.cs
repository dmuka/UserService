using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApi.Pages.Shared;

public class _SignInPartial : PageModel
{
    public void OnGet()
    {
        Console.WriteLine("Hello from _SignInPartial");
    }
}