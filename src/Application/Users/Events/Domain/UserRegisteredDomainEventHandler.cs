using Application.Abstractions.Email;
using Application.Abstractions.Kafka;
using Domain;
using Domain.Users.Events.Domain;
using Domain.Users.Events.Integration;

namespace Application.Users.Events.Domain;

/// <summary>
/// Handles the UserRegisteredDomainEvent.
/// </summary>
public class UserRegisteredDomainEventHandler(
    ITokenHandler tokenHandler,
    IUrlGenerator urlGenerator,
    IEmailService emailService,
    IEventPublisher eventPublisher) : IEventHandler<UserRegisteredDomainEvent>
{
    public async Task HandleAsync(UserRegisteredDomainEvent @event, CancellationToken cancellationToken = default)
    { 
        var token = tokenHandler.GetEmailToken(@event.UserId.Value.ToString());
        var confirmationLink = urlGenerator.GenerateEmailConfirmationLink(@event.UserId, token);
            
        var emailBody = $"<p>Please confirm your email by clicking <a href='{confirmationLink}'>here</a>.</p>";
            
        await emailService.SendEmailAsync(@event.Email, "Confirm your email", emailBody);
        
        var userRegisteredIntegrationEvent = new UserRegisteredIntegrationEvent(
            @event.UserId,
            @event.FirstName,
            @event.LastName,
            @event.Email, 
            @event.RegisteredAt);
        await eventPublisher.PublishAsync("user-registered", userRegisteredIntegrationEvent, cancellationToken);
    }
}