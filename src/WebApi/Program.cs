using System.Net;
using Application;
using Grpc.Services;
using HealthChecks.UI.Client;
using Infrastructure;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Scalar.AspNetCore;
using Serilog;
using WebApi;
using WebApi.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, loggerConfig) => loggerConfig.ReadFrom.Configuration(context.Configuration));

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services
    .AddApplication()
    .AddPresentation()
    .AddInfrastructure(builder.Configuration);

builder.Services.AddRazorPages();
builder.Services.AddAntiforgery(options => 
{
    options.Cookie.Name = ".AspNetCore.Antiforgery";
    options.HeaderName = "X-CSRF-TOKEN";
});

var app = builder.Build();

app.MapEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseStatusCodePages(context =>
{
    var response = context.HttpContext.Response;

    switch(response.StatusCode)
    {
        case (int)HttpStatusCode.Forbidden:
            response.Redirect("/AccessDenied");
            break;
        case (int)HttpStatusCode.Unauthorized:
            response.Redirect("/SignIn");
            break;
    }

    return Task.CompletedTask;
});

app.Use(async (context, next) =>
{
    var token = context.Request.Cookies["AccessToken"];
    if (!string.IsNullOrEmpty(token))
    {
        context.Request.Headers.Append("Authorization", $"Bearer {token}");
    }
    await next();
});

app.MapHealthChecks("healthch", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

// Prevent 404 log entry
app.Use(async (context, next) =>
{
    if (context.Request.Path == "/favicon.ico")
    {
        context.Response.StatusCode = StatusCodes.Status204NoContent;
        return;
    }

    await next();
});

app.UseRequestContextLogging();

app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseAuthentication();

app.UseAuthorization();

app.MapGrpcService<UserGrpcService>();

app.MapRazorPages();

await app.RunAsync();