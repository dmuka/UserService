using Application.Abstractions.Messaging;

namespace Application.Roles.GetByUserId;

public sealed record GetRolesByUserIdQuery(Guid UserId) : IQuery<RolesResponse>;