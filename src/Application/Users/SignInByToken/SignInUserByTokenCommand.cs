using Application.Abstractions.Messaging;

namespace Application.Users.SignInByToken;

public sealed record SignInUserByTokenCommand(string RefreshToken) : ICommand<SignInUserByTokenResponse>;
