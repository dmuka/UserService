using Application.Abstractions.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WebApi.Infrastructure;

public class TokenAuthFilter(
    TokenHandler tokenHandler, 
    ITokenProvider tokenProvider) : IAsyncPageFilter
{
    public async Task OnPageHandlerExecutionAsync(
        PageHandlerExecutingContext context, 
        PageHandlerExecutionDelegate next)
    {
        if (context.HttpContext.Request.Path.StartsWithSegments("/SignIn")
            || context.HttpContext.Request.Path.StartsWithSegments("/SignUp"))
        {
            await next();
            return;
        }

        var accessToken = tokenHandler.GetAccessToken();
        
        if (string.IsNullOrEmpty(accessToken))
        {
            context.Result = new RedirectToPageResult("/SignIn");
            return;
        }

        if (!IsTokenValid(accessToken))
        {
            if (!await tokenHandler.RefreshTokens())
            {
                context.Result = new RedirectToPageResult("/SignIn");
                return;
            }
        }

        await next();
    }

    private bool IsTokenValid(string token)
    {
        return tokenProvider.ValidateAccessToken(token);
    }

    public Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context)
    {
        return Task.CompletedTask;
    }
}