 using System.IdentityModel.Tokens.Jwt;
 using System.Security.Claims;
 using System.Text;
 using Application.Abstractions.Authentication;
 using Application.Abstractions.Email;
 using Application.Abstractions.Messaging;
 using Application.Users.SignInByToken;
 using Domain;
 using Domain.Roles;
 using Domain.Users;
 using Infrastructure.Authentication;
 using Infrastructure.Authorization;
 using Infrastructure.Caching;
 using Infrastructure.Caching.Interfaces;
 using Infrastructure.Email;
 using Infrastructure.Events;
 using Infrastructure.HealthChecks;
 using Infrastructure.Options.Authentication;
 using Infrastructure.Options.Db;
 using Infrastructure.Options.Email;
 using Infrastructure.Repositories;
 using Microsoft.AspNetCore.Authentication.JwtBearer;
 using Microsoft.AspNetCore.Authorization;
 using Microsoft.Extensions.Configuration;
 using Microsoft.Extensions.DependencyInjection;
 using Microsoft.IdentityModel.Tokens;
 using Serilog;

 namespace Infrastructure;

 public static class Di
 {
     public static IServiceCollection AddInfrastructure(
         this IServiceCollection services,
         IConfiguration configuration) =>
         services
             .AddAuthentication(configuration)
             .AddAuthorizationLogic()
             .AddDbConnectionOptions()
             .AddHealthCheck()
             .AddEmailService()
             .AddRepositories()
             .AddCache()
             .AddEventDispatcher();

     private static IServiceCollection AddCache(this IServiceCollection services)
     {
         services.AddMemoryCache();
         services.AddResponseCaching();

         services.AddScoped<ICacheService, CacheService>();
         
         return services;
     }

     private static IServiceCollection AddEventDispatcher(this IServiceCollection services)
     {
         services.AddScoped<IEventDispatcher, EventDispatcher>();
         
         return services;
     }
     
     private static IServiceCollection AddRepositories(this IServiceCollection services)
     {
         services.AddScoped<IUserRepository, UserRepository>();
         services.AddScoped<IRoleRepository, RoleRepository>();
         services.AddScoped<IUserRoleRepository, UserRoleRepository>();
         services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
         
         return services;
     }    
     
     private static IServiceCollection AddDbConnectionOptions(this IServiceCollection services)
     {
         services.AddOptions<PostgresOptions>()
             .BindConfiguration("DbConnections:Postgres")
             .ValidateDataAnnotations()
             .ValidateOnStart();
         
         return services;
     }
     
     private static IServiceCollection AddAuthentication(
         this IServiceCollection services,
         IConfiguration configuration)
     {
         services.AddOptions<AuthOptions>()
             .BindConfiguration("Jwt")
             .ValidateDataAnnotations()
             .ValidateOnStart();
         
         services.AddScoped<ICommandHandler<SignInUserByTokenCommand, SignInUserByTokenResponse>, SignInUserByTokenCommandHandler>();
         //var authOptions = configuration.Get<AuthOptions>();
         
         services
             .AddAuthentication(options =>
             {
                 options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                 options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
             })
             //.AddCookie(options => options.LoginPath = "/SignIn")
             .AddJwtBearer(jwtBearerOptions =>
             {
                 jwtBearerOptions.RequireHttpsMetadata = false;
                 jwtBearerOptions.SaveToken = true;
                 jwtBearerOptions.TokenValidationParameters = new TokenValidationParameters
                 {
                     IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Secret"]!)),
                     ValidIssuer = configuration["Jwt:Issuer"],
                     ValidAudience = configuration["Jwt:Audience"],
                     ClockSkew = TimeSpan.Zero
                 };
                 jwtBearerOptions.Events = new JwtBearerEvents
                 {
                     OnTokenValidated = context =>
                     {
                         var user = context.Principal;
                         Log.Information("Token validated for user: {UserName}", user?.Identity?.Name);
                         return Task.CompletedTask;
                     },
                     OnAuthenticationFailed = async context =>
                     {
                         var user = context.Principal;
                         
                         if (context.Exception is SecurityTokenExpiredException)
                         {
                             Log.Information("Token expired, attempting to renew.");

                             var sessionId = context.HttpContext.Request.Cookies["SessionId"];
                             
                             if (!string.IsNullOrEmpty(sessionId))
                             {
                                 var refreshTokenRepository = context.HttpContext.RequestServices.GetRequiredService<IRefreshTokenRepository>();
                                 var refreshToken = await refreshTokenRepository.GetTokenByIdAsync(Guid.Parse(sessionId));

                                 if (refreshToken != null && refreshToken.ExpiresUtc > DateTime.UtcNow)
                                 {
                                     var refreshTokenHandler = context.HttpContext.RequestServices
                                         .GetRequiredService<ICommandHandler<SignInUserByTokenCommand, SignInUserByTokenResponse>>();

                                     var command = new SignInUserByTokenCommand(refreshToken.Value);
                                     var result = await refreshTokenHandler.Handle(command, CancellationToken.None);

                                     if (result.IsSuccess)
                                     {
                                         context.HttpContext.Request.Headers.Authorization =
                                              $"Bearer {result.Value.AccessToken}";                        
                                         
                                         var tokenHandler = new JwtSecurityTokenHandler();
                                         var jwtToken = tokenHandler.ReadJwtToken(result.Value.AccessToken);
                                         var identity = new ClaimsIdentity(jwtToken.Claims, "Bearer");
                                         var principal = new ClaimsPrincipal(identity);
                                         context.HttpContext.User = principal;
                                         
                                         context.HttpContext.Response.Cookies.Append("SessionId",
                                             result.Value.SessionId.ToString());
                                         
                                         context.HttpContext.Response.Cookies.Append("AccessToken",
                                             result.Value.AccessToken);
                                         
                                         context.Fail($"Token renewed successfully.");
                                         return;
                                     }
                                 }
                             }
                         }
                         
                         Log.Error(context.Exception, "Authentication failed");
                     }
                 };
             });
         
         services.AddHttpContextAccessor();
         services.AddScoped<IUserContext, UserContext>();
         services.AddSingleton<ITokenProvider, TokenProvider>();
         services.AddScoped<IPasswordHasher, PasswordHasher>();

         return services;
     }

     private static IServiceCollection AddHealthCheck(this IServiceCollection services)
     {
         services.AddHealthChecks()
             .AddCheck<PostgresHealthCheck>(nameof(PostgresHealthCheck), tags: ["postgres"])
             .AddCheck<CacheHealthCheck>(nameof(CacheHealthCheck), tags: ["cache"]);

         services.AddTransient<INpgsqlConnectionFactory, NpgsqlConnectionFactory>();
         
         return services;
     }

     private static IServiceCollection AddEmailService(this IServiceCollection services)
     {
         services.AddOptions<SmtpOptions>()
             .BindConfiguration("SmtpOptions")
             .ValidateDataAnnotations()
             .ValidateOnStart();
         
         services.AddTransient<IEmailService, EmailService>();
         
         return services;
     }

     private static IServiceCollection AddAuthorizationLogic(this IServiceCollection services)
     {
         services.AddAuthorizationBuilder()
             .AddPolicy(
                 "UserManagementPolicy", 
                 configurePolicy => configurePolicy.RequireRole("Admin", "Manager"));
         services.AddSingleton<IAuthorizationHandler, RoleAuthorizationHandler>();
         services.ConfigureApplicationCookie(options =>
         {
             options.AccessDeniedPath = "/AccessDenied";
         });
         
         services.AddOpenApi(options => options.AddDocumentTransformer<BearerSecuritySchemeTransformer>());

         services.AddScoped<PermissionProvider>();

         return services;
     }
 }
