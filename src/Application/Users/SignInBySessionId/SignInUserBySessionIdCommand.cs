using Application.Abstractions.Messaging;

namespace Application.Users.SignInBySessionId;

public sealed record SignInUserBySessionIdCommand(Guid SessionId) : ICommand<SignInUserBySessionIdResponse>;
