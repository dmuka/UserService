using Application.Abstractions.Messaging;
using Domain.Roles;
using Domain.Users;

namespace Application.Roles.Update;

public sealed record UpdateRoleCommand(Role Role) : ICommand<int>;