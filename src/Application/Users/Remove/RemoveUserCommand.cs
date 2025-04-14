using Application.Abstractions.Messaging;

namespace Application.Users.Remove;

public sealed record RemoveUserCommand(Guid UserId) : ICommand<int>;