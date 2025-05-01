using Application.Abstractions.Messaging;

namespace Application.Users.SignIn;

public sealed record SignInUserCommand(
    string Username, 
    string Password,
    int TokenExpirationInDays,
    string? Email = null) : ICommand<SignInResponse>;
