using System.Net;
using Grpc.Services;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using WebApi.Infrastructure;
using WebApi.Middleware;
using WebApi.Pages;

namespace WebApi.Extensions;

/// <summary>
/// Extension methods for adding middleware to the application's request pipeline.
/// </summary>
public static class MiddlewareExtensions
{
    /// <summary>
    /// Adds the <see cref="RequestContextLoggingMiddleware"/> to the application's request pipeline.
    /// </summary>
    /// <param name="app">The <see cref="IApplicationBuilder"/> to configure.</param>
    /// <returns>The <see cref="IApplicationBuilder"/> with the middleware added.</returns>
    public static IApplicationBuilder UseRequestContextLogging(this IApplicationBuilder app)
    {
        app.UseMiddleware<RequestContextLoggingMiddleware>();

        return app;
    }

    public static IApplicationBuilder UseStatusCodePagesMiddleware(this IApplicationBuilder app)
    {
        app.UseStatusCodePages(context =>
        {
            var response = context.HttpContext.Response;

            switch (response.StatusCode)
            {
                case (int)HttpStatusCode.Forbidden:
                    response.Redirect(Routes.Denied403);
                    break;
                case (int)HttpStatusCode.Unauthorized:
                    response.Redirect(Routes.SignIn);
                    break;
            }

            return Task.CompletedTask;
        });
        
        return app;
    }

    public static IApplicationBuilder AddAuthorizationHeader(this IApplicationBuilder app)
    {
        app.Use(async (context, next) =>
        {
            var token = context.Request.Cookies[CookiesNames.AccessToken];
            if (!string.IsNullOrEmpty(token))
            {
                context.Request.Headers.Append("Authorization", $"Bearer {token}");
            }
            await next();
        });
        
        return app;
    }

    public static WebApplication AddHealthChecks(this WebApplication app)
    {
        app.MapHealthChecks("healthch", new HealthCheckOptions
        {
            Predicate = _ => true,
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });
        
        return app;
    }

    public static WebApplication MapLocalGrpcServices(this WebApplication app)
    {
        app.MapGrpcService<UserGrpcService>();
        
        return app;
    }
}
