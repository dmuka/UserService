using System.Net;
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

var certificatePath = Environment.GetEnvironmentVariable("CertificatePath")
                      ?? throw new InvalidOperationException("Certificate path is not set.");            
var certificatePassword = Environment.GetEnvironmentVariable("CertificatePassword")
             ?? throw new InvalidOperationException("Certificate password is not set.");

Host.CreateDefaultBuilder(args)
    .ConfigureWebHostDefaults(webBuilder =>
    {
        webBuilder.UseStartup<Program>()
            .UseKestrel(options =>
            {
                options.Listen(IPAddress.Any, 5000);
                options.Listen(IPAddress.Any, 5001, listenOptions =>
                {
                    listenOptions.UseHttps(certificatePath, certificatePassword);
                });
                
            });
    });

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
        var vaultOrganizationId = builder.Configuration["Vault:OrganizationId"];
        if (vaultOrganizationId is null) throw new InvalidOperationException("Vault organization id is not set.");
        source.OrganizationId = vaultOrganizationId;
    });
}
else
{
    builder.Configuration.Add<SecretsConfigurationSource>(source =>
    {
        var identityUrl = builder.Configuration["Vault:IdentityUrl"] 
                          ?? (Environment.GetEnvironmentVariable("Vault__IdentityUrl") 
                              ?? throw new InvalidOperationException("Identity URL is not set."));
        source.IdentityUrl = identityUrl;
            
        var apiUrl = builder.Configuration["Vault:ApiUrl"] 
                     ?? (Environment.GetEnvironmentVariable("Vault__ApiUrl") 
                         ?? throw new InvalidOperationException("API URL is not set."));
        source.ApiUrl = apiUrl;

        var vaultAccessToken = builder.Configuration["Vault:AccessToken"]
                               ?? (Environment.GetEnvironmentVariable("Vault__AccessToken")
                                   ?? throw new InvalidOperationException("Vault access token is not set."));
        source.AccessToken = vaultAccessToken;
        
        var vaultOrganizationId = builder.Configuration["Vault:OrganizationId"]
                                  ?? (Environment.GetEnvironmentVariable("Vault__OrganizationId")
                                      ?? throw new InvalidOperationException("Vault organization id is not set."));
        source.OrganizationId = vaultOrganizationId;
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