using Application.Abstractions.Messaging;

namespace Application.Roles.GetByName;

public sealed record GetRoleByNameQuery(string RoleName) : IQuery<RoleResponse>;