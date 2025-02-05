using WebApi.Infrastructure;

namespace WebApi;

public static class Di
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddProblemDetails();

        services.AddExceptionHandler<GlobalExceptionHandler>();
        
        return services;
    }
}
