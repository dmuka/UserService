using Application.Abstractions.Behaviors;
using Application.Abstractions.Kafka;
using Application.Users.Events.Domain;
using Domain;
using Domain.Users.Events.Domain;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;

namespace Application;

public static class Di
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(typeof(Di).Assembly);            
            
            config.AddOpenBehavior(typeof(RequestLoggingPipelineBehavior<,>));
            config.AddOpenBehavior(typeof(ValidationPipelineBehavior<,>));
        });

        services.AddTransient<IEventHandler<UserRegisteredDomainEvent>, UserRegisteredDomainEventHandler>();

        services.AddValidatorsFromAssembly(typeof(Di).Assembly, includeInternalTypes: true);

        return services;
    }
}
