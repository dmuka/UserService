using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class Di
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(typeof(Di).Assembly);
        });

        return services;
    }
}
