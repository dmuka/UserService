using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Core;
using Domain.Roles;
using Domain.Users;

namespace Application.Roles.GetByUserId;

public class GetRolesByUserIdQueryHandler(IRoleRepository repository, IUserContext userContext) 
    : IQueryHandler<GetRolesByUserIdQuery, RolesResponse>
{
    public async Task<Result<RolesResponse>> Handle(
        GetRolesByUserIdQuery query, 
        CancellationToken cancellationToken)
    {
        var roles = await repository.GetRolesByUserIdAsync(query.UserId, cancellationToken);

        if (roles.Count == 0)
        {
            return Result.Failure<RolesResponse>(UserErrors.NotFound(query.UserId));
        }
        
        var roleResponse = RolesResponse.Create(roles);

        return roleResponse;
    }
}