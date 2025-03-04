using Core;
using Domain.Roles;

namespace Domain.Users.Specifications;

public class UserMustHaveAtLeastOneRoleAfterRemoveRole(ICollection<RoleId> roleIds) : ISpecification
{
    public Result IsSatisfied()
    {
        return roleIds.Count < 2 
            ? Result.Failure<ICollection<RoleId>>(UserErrors.LastRoleRemove) 
            : Result.Success();
    }
}