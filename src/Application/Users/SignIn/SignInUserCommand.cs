using Application.Abstractions.Messaging;

namespace Application.Users.SignIn;

public sealed record SignInUserCommand(
    string Username, 
    string Password,
    string? Email = null) : ICommand<string>;
