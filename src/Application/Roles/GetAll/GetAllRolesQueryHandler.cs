using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Core;
using Domain.Roles;

namespace Application.Roles.GetAll;

public class GetAllRolesQueryHandler(IRoleRepository repository, IUserContext userContext) 
    : IQueryHandler<GetAllRolesQuery, IList<RoleResponse>>
{
    public async Task<Result<IList<RoleResponse>>> Handle(
        GetAllRolesQuery query, 
        CancellationToken cancellationToken)
    {
        if (userContext.UserRole != "Admin")
        {
            return Result.Failure<IList<RoleResponse>>(RoleErrors.Unauthorized());
        }
        
        var roles = await repository.GetAllRolesAsync(cancellationToken);
        
        var rolesResponse = roles
            .AsParallel()
            .AsOrdered()
            .Select(RoleResponse.Create)
            .ToList();

        return rolesResponse;
    }
}