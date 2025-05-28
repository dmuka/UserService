using System.Web;
using Application.Abstractions.Email;
using Domain;
using Domain.Users;
using Domain.Users.DomainEvents;

namespace Application.Users.Events;

/// <summary>
/// Handles the UserRegisteredDomainEvent.
/// </summary>
public class UserRegisteredDomainEventHandler(
    ITokenHandler tokenHandler,
    IUrlGenerator urlGenerator,
    IEmailService emailService) : IEventHandler<UserRegisteredDomainEvent>
{
    public async Task HandleAsync(UserRegisteredDomainEvent @event, CancellationToken cancellationToken = default)
    { 
        var token = tokenHandler.GetEmailToken(@event.UserId.Value.ToString());
        var confirmationLink = urlGenerator.GenerateEmailConfirmationLink(@event.UserId, token);
            
        var emailBody = $"<p>Please confirm your email by clicking <a href='{confirmationLink}'>here</a>.</p>";
            
        await emailService.SendEmailAsync(@event.Email, "Confirm your email", emailBody);
    }
}