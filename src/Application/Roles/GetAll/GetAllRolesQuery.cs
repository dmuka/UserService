using Application.Abstractions.Messaging;

namespace Application.Roles.GetAll;

public sealed record GetAllRolesQuery : IQuery<IList<RoleResponse>>;