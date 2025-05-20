using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Core;
using Domain.Roles;

namespace Application.Roles.GetByName;

public class GetRoleByNameQueryHandler(IRoleRepository repository) 
    : IQueryHandler<GetRoleByNameQuery, RoleResponse>
{
    public async Task<Result<RoleResponse>> Handle(
        GetRoleByNameQuery query, 
        CancellationToken cancellationToken)
    {
        var role = await repository.GetRoleByNameAsync(query.RoleName, cancellationToken);

        if (role is null)
        {
            return Result.Failure<RoleResponse>(RoleErrors.NotFound(query.RoleName));
        }
        
        var roleResponse = RoleResponse.Create(role);

        return roleResponse;
    }
}