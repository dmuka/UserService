using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Core;
using Domain.Roles;

namespace Application.Roles.GetAll;

public class GetAllRolesQueryHandler(IRoleRepository repository, IUserContext userContext) 
    : IQueryHandler<GetAllRolesQuery, IEnumerable<RoleResponse>>
{
    public async Task<Result<IEnumerable<RoleResponse>>> Handle(
        GetAllRolesQuery query, 
        CancellationToken cancellationToken)
    {
        // if (userContext.UserRole != "Admin")
        // {
        //     return Result.Failure<IEnumerable<RoleResponse>>(RoleErrors.Unauthorized());
        // }
        
        var roles = await repository.GetAllRolesAsync(cancellationToken);
        
        var rolesResponse = roles
            .AsParallel()
            .AsOrdered()
            .Select(RoleResponse.Create);

        return rolesResponse;
    }
}