using System.Reflection;
using Grpc.Infrastructure.Interceptors;
using WebApi.Extensions;
using WebApi.Infrastructure;

namespace WebApi;

public static class Di
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services.AddEndpoints(Assembly.GetExecutingAssembly());
        services.AddGrpc(options =>
        {
            options.EnableDetailedErrors = true;
            options.Interceptors.Add<LoggingInterceptor>();
        });
        services.AddEndpointsApiExplorer();

        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();
        
        return services;
    }
}
