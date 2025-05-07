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
             .AddEmailService(configuration)
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
         
         services
             .AddAuthentication(options =>
             {
                 options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                 options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
             })
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
                         foreach (var claim in user?.Claims)
                         {
                             Log.Information("Principal claim type: {ClaimType}, claim value: {ClaimValue}", claim.Type, claim.Value);
                         }
                         
                         return Task.CompletedTask;
                     },
                     OnAuthenticationFailed = async context =>
                     {
                         var user = context.Principal;
                         
                         Log.Error(context.Exception, "Authentication failed ({User})", user?.Identity is null ? "Unknown" : user.Identity.Name);
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

     private static IServiceCollection AddEmailService(this IServiceCollection services, IConfiguration configuration)
     {
         services.AddOptions<SmtpOptions>()
             .BindConfiguration("SmtpOptions")
             .ValidateDataAnnotations()
             .ValidateOnStart();
         
         services.Configure<SmtpOptions>(options =>
         {
             var userName = configuration["SmtpOptionsSecrets:Username"];
             var password = configuration["SmtpOptionsSecrets:Password"];
             
             if (string.IsNullOrWhiteSpace(userName) || userName.Length < 3 || userName.Length > 50)
             {
                 throw new InvalidOperationException("SMTP username is invalid. Please check your configuration.");
             }

             if (string.IsNullOrWhiteSpace(password) || password.Length < 6 || password.Length > 100)
             {
                 throw new InvalidOperationException("SMTP password is invalid. Please check your configuration.");
             }
             
             options.UserName = userName;
             options.Password = password;
         });
         
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
