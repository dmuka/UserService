 using Application.Abstractions.Authentication;
 using Infrastructure.Authentication;
 using Microsoft.Extensions.Configuration;
 using Microsoft.Extensions.DependencyInjection; 

 namespace Infrastructure;

 public static class Di
 {
     public static IServiceCollection AddInfrastructure(
         this IServiceCollection services,
         IConfiguration configuration) =>
         services
             .AddAuthentication();
     
    private static IServiceCollection AddAuthentication(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<IUserContext, UserContext>();

        return services;
    }
 }
