using Application.Abstractions.Messaging;

namespace Application.Users.EnableMfa;

public sealed record EnableMfaCommand(string UserId, int VerificationCode) : ICommand<List<string>>;