using Application.Abstractions.Messaging;
using Core;
using Domain.Users;

namespace Application.Users.ConfirmEmail;

internal sealed class ConfirmEmailCommandHandler(IUserRepository userRepository) : ICommandHandler<ConfirmEmailCommand>
{
    public async Task<Result> Handle(ConfirmEmailCommand command, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(command.UserId, out var userId))
        {
            return Result.Failure(UserErrors.InvalidUserId);
        }
        
        var user = await userRepository.GetUserByIdAsync(userId, cancellationToken);
        if (user is null) return Result.Failure(UserErrors.NotFound(userId));
        
        var result = user.ConfirmEmail();
        if (result.IsFailure) return Result.Failure(UserErrors.UserEmailConfirmationError);
        
        await userRepository.UpdateUserAsync(user, cancellationToken);
        
        return Result.Success();
    }
}