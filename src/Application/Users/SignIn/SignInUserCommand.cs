using Application.Abstractions.Messaging;

namespace Application.Users.SignIn;

public sealed record SignInUserCommand(
    string Username, 
    string Password,
    bool RememberMe,
    int TokenExpirationInDays,
    string? VerificationCode = null,
    string? RecoveryCode = null,
    string? Email = null) : ICommand<SignInResponse>;
