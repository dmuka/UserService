using Application;
using Infrastructure;
using Infrastructure.Options.Email;
using Infrastructure.Vault;
using Scalar.AspNetCore;
using Serilog;
using WebApi;
using WebApi.Extensions;
using WebApi.Middleware;

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

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<SmtpOptions>();
    builder.Configuration.Add<SecretsConfigurationSource>(source =>
    {
        var identityUrl = builder.Configuration["Vault:IdentityUrl"];
        if (identityUrl is null) throw new InvalidOperationException("Identity URL is not set.");
        source.IdentityUrl = identityUrl;
        
        var apiUrl = builder.Configuration["Vault:ApiUrl"];
        if (apiUrl is null) throw new InvalidOperationException("API URL is not set.");
        source.ApiUrl = apiUrl;
        
        var vaultAccessToken = builder.Configuration["Vault:AccessToken"];
        if (vaultAccessToken is null) throw new InvalidOperationException("Vault access token is not set.");
        source.AccessToken = vaultAccessToken;
            // Environment.GetEnvironmentVariable("VAULT_ACCESS_TOKEN")
            //                  ?? throw new InvalidOperationException("Vault access token not set.");
        var vaultOrganizationId = builder.Configuration["Vault:OrganizationId"];
        if (vaultOrganizationId is null) throw new InvalidOperationException("Vault organization id is not set.");
        source.OrganizationId = vaultOrganizationId;
            // Environment.GetEnvironmentVariable("VAULT_ORGANIZATION_ID")
            //                     ?? throw new InvalidOperationException("Vault organization id not set.");
    });
}

var app = builder.Build();

app.MapEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseStatusCodePagesMiddleware();

app.AddHealthChecks();

app.UseRequestContextLogging();
app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

app.UseStaticFiles();

app.AddAuthorizationHeader();
app.UseMiddleware<TokenRenewalMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapLocalGrpcServices();

app.MapRazorPages();

await app.RunAsync();