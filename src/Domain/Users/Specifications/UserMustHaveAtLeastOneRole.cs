using Core;
using Domain.Roles;

namespace Domain.Users.Specifications;

public class UserMustHaveAtLeastOneRole(ICollection<RoleId> roleIds) : ISpecification
{
    public Result IsSatisfied()
    {
        if (roleIds is null) return Result.Failure(Error.NullValue);
        
        return roleIds.Count == 0 
            ? Result.Failure<ICollection<RoleId>>(UserErrors.EmptyRolesCollection) 
            : Result.Success();
    }
}