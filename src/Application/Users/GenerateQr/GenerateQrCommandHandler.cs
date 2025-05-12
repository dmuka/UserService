using System.Reflection;
using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Core;
using Domain.Users;
using Domain.ValueObjects.MfaSecrets;
using Microsoft.Extensions.Logging;

namespace Application.Users.GenerateQr;

internal sealed class GenerateQrCommandHandler(
    IUserRepository userRepository, 
    ITotpProvider totpProvider,
    ILogger<GenerateQrCommandHandler> logger) : ICommandHandler<GenerateQrCommand, string>
{
    public async Task<Result<string>> Handle(GenerateQrCommand command, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(command.UserId, out var userId))
        {
            return Result.Failure<string>(UserErrors.InvalidUserId);
        }
        
        var user = await userRepository.GetUserByIdAsync(userId, cancellationToken);
        if (user is null) return Result.Failure<string>(UserErrors.NotFound(userId));

        if (user.MfaSecret is null)
        {
            var secretKey = totpProvider.GenerateSecretKey();
            var result = user.SetupMfa(MfaSecret.Create(secretKey));
            if (result.IsFailure) return Result.Failure<string>(UserErrors.InvalidMfaSecret);
            await userRepository.UpdateUserAsync(user, cancellationToken);
        }

        logger.LogInformation("QR Code Secret: {secret}", user.MfaSecret);
        
        var qr = totpProvider.GetQr(user.MfaSecret, user.Email, Assembly.GetEntryAssembly()?.GetName().Name ?? "Standards");
            
        return qr;
    }
}