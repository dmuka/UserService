using Application.Abstractions.Messaging;

namespace Application.Users.ConfirmEmail;

public sealed record ConfirmEmailCommand(string UserId) : ICommand;