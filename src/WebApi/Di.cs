using System.Reflection;
using Application.Abstractions.Email;
using Grpc.Infrastructure.Interceptors;
using Infrastructure.Email;
using Microsoft.Extensions.DependencyInjection.Extensions;
using WebApi.Infrastructure;

namespace WebApi;

public static class Di
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services
            .AddGrpcServices()
            .AddEndpoints(Assembly.GetExecutingAssembly())
            .AddFilters()
            .AddAntiforgeryServices()
            .AddEndpointsApiExplorer()
            .AddExceptionHandler<GlobalExceptionHandler>()
            .AddProblemDetails()
            .AddHttpClient()
            .AddPresentationServices();
        
        return services;
    }

    private static IServiceCollection AddGrpcServices(this IServiceCollection services)
    {
        services.AddGrpc(options =>
        {
            options.EnableDetailedErrors = true;
            options.Interceptors.Add<LoggingInterceptor>();
        });
         
        return services;
    }
    
    private static IServiceCollection AddEndpoints(this IServiceCollection services, Assembly assembly)
    {
        var serviceDescriptors = assembly
            .DefinedTypes
            .Where(type => type is { IsAbstract: false, IsInterface: false } &&
                           type.IsAssignableTo(typeof(IEndpoint)))
            .Select(type => ServiceDescriptor.Transient(typeof(IEndpoint), type))
            .ToArray();

        services.TryAddEnumerable(serviceDescriptors);

        return services;
    }
    
    private static IServiceCollection AddPresentationServices(this IServiceCollection services)
    {
        services.AddScoped<ITokenHandler, TokenHandler>();
        services.AddSingleton<IUrlGenerator, UrlGenerator>();

        return services;
    }
    
    private static IServiceCollection AddFilters(this IServiceCollection services)
    {
        services.AddMvc(options =>
        {
            options.Filters.Add<TokenAuthFilter>();
        });

        return services;
    }
    
    private static IServiceCollection AddAntiforgeryServices(this IServiceCollection services)
    {
        services.AddAntiforgery();

        return services;
    }
}
