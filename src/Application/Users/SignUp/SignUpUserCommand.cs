using Application.Abstractions.Messaging;

namespace Application.Users.SignUp;

public sealed record SignUpUserCommand(
    string Username,
    string Email, 
    string FirstName, 
    string LastName, 
    string Password,
    IEnumerable<Guid>? RolesIds = null)
    : ICommand<Guid>;
