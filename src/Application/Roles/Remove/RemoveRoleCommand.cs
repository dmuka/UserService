using Application.Abstractions.Messaging;

namespace Application.Roles.RemoveRole;

public sealed record RemoveRoleCommand(Guid RoleId) : ICommand<int>;