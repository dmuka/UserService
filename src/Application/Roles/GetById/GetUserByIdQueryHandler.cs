using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Core;
using Domain.Roles;
using Domain.Users;

namespace Application.Roles.GetById;

public class GetRoleByIdQueryHandler(IRoleRepository repository, IUserContext userContext) 
    : IQueryHandler<GetRoleByIdQuery, RoleResponse>
{
    public async Task<Result<RoleResponse>> Handle(
        GetRoleByIdQuery query, 
        CancellationToken cancellationToken)
    {
        var role = await repository.GetRoleByIdAsync(query.RoleId, cancellationToken);

        if (role is null)
        {
            return Result.Failure<RoleResponse>(UserErrors.NotFound(query.RoleId));
        }
        
        var roleResponse = RoleResponse.Create(role);

        return roleResponse;
    }
}