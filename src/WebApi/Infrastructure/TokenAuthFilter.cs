using Application.Abstractions.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using WebApi.Pages;

namespace WebApi.Infrastructure;

public class TokenAuthFilter(
    TokenHandler tokenHandler, 
    ITokenProvider tokenProvider) : IAsyncPageFilter
{
    public async Task OnPageHandlerExecutionAsync(
        PageHandlerExecutingContext context, 
        PageHandlerExecutionDelegate next)
    {
        if (context.HttpContext.Request.Path.StartsWithSegments(Routes.SignIn)
            || context.HttpContext.Request.Path.StartsWithSegments(Routes.SignUp))
        {
            await next();
            return;
        }

        var accessToken = tokenHandler.GetAccessToken();
        
        if (string.IsNullOrEmpty(accessToken))
        {
            context.Result = new RedirectToPageResult(Routes.SignIn);
            return;
        }

        // if (!IsTokenValid(accessToken))
        // {
        //     if (!await tokenHandler.RefreshTokens())
        //     {
        //         context.Result = new RedirectToPageResult(Routes.SignIn);
        //         return;
        //     }
        // }
        
        await next();
    }

    private bool IsTokenValid(string? token)
    {
        return tokenProvider.ValidateAccessToken(token);
    }

    public Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context)
    {
        return Task.CompletedTask;
    }
}