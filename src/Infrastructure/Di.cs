 using Application.Abstractions.Authentication;
 using Domain.Users;
 using Infrastructure.Authentication;
 using Infrastructure.Options.Db;
 using Infrastructure.Repositories;
 using Microsoft.Extensions.Configuration;
 using Microsoft.Extensions.DependencyInjection; 

 namespace Infrastructure;

 public static class Di
 {
     public static IServiceCollection AddInfrastructure(
         this IServiceCollection services,
         IConfiguration configuration) =>
         services
             .AddAuthentication()
             .AddRepositories();

     private static IServiceCollection AddRepositories(this IServiceCollection services)
     {
         services.AddScoped<IUserRepository, UserRepository>();
         
         return services;
     }
     
     private static IServiceCollection AddAuthentication(this IServiceCollection services)
     {
         services.AddHttpContextAccessor();
         services.AddScoped<IUserContext, UserContext>();

         return services;
     }
 }
