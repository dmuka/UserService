using Application.Abstractions.Messaging;
using Application.Roles.GetByUserId;

namespace Application.Roles.GetById;

public sealed record GetRoleByIdQuery(Guid RoleId) : IQuery<RoleResponse>;