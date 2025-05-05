using Application.Abstractions.Messaging;

namespace Application.Roles.Remove;

public sealed record RemoveRoleCommand(Guid RoleId) : ICommand<int>;