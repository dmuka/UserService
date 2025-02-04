 using System.Text;
 using Application.Abstractions.Authentication;
 using Domain.Roles;
 using Domain.Users;
 using Infrastructure.Authentication;
 using Infrastructure.Options.Authentication;
 using Infrastructure.Options.Db;
 using Infrastructure.Repositories;
 using Microsoft.AspNetCore.Authentication.JwtBearer;
 using Microsoft.Extensions.Configuration;
 using Microsoft.Extensions.DependencyInjection;
 using Microsoft.Extensions.Options;
 using Microsoft.IdentityModel.Tokens;

 namespace Infrastructure;

 public static class Di
 {
     public static IServiceCollection AddInfrastructure(
         this IServiceCollection services,
         IConfiguration configuration) =>
         services
             .AddAuthentication(configuration)
             .AddDbConnectionOptions()
             .AddRepositories();

     private static IServiceCollection AddRepositories(this IServiceCollection services)
     {
         services.AddScoped<IUserRepository, UserRepository>();
         services.AddScoped<IRoleRepository, RoleRepository>();
         
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
         
         //var authOptions = configuration.Get<AuthOptions>();
         
         services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
             .AddJwtBearer(jwtBearerOptions =>
             {
                 jwtBearerOptions.RequireHttpsMetadata = false;
                 jwtBearerOptions.TokenValidationParameters = new TokenValidationParameters
                 {
                     IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Secret"]!)),
                     ValidIssuer = configuration["Jwt:Issuer"],
                     ValidAudience = configuration["Jwt:Audience"],
                     ClockSkew = TimeSpan.Zero
                 };
             });
         
         services.AddHttpContextAccessor();
         services.AddScoped<IUserContext, UserContext>();
         services.AddScoped<IPasswordHasher, PasswordHasher>();

         return services;
     }
 }
