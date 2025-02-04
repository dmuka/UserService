using Application.Abstractions.Messaging;
using Application.Users.GetById;

namespace Application.Roles.GetById;

public sealed record GetRoleByIdQuery(Guid RoleId) : IQuery<RoleResponse>;