using Application.Abstractions.Messaging;
using Domain.Users;

namespace Application.Users.Update;

public sealed record UpdateUserCommand(User User) : ICommand<int>;