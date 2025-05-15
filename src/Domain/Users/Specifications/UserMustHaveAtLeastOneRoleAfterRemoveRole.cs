using Core;
using Domain.ValueObjects.RoleNames;

namespace Domain.Users.Specifications;

public class UserMustHaveAtLeastOneRoleAfterRemoveRole(ICollection<RoleName> roleNames) : ISpecification
{
    public Result IsSatisfied()
    {
        return roleNames.Count < 2 
            ? Result.Failure<ICollection<RoleName>>(UserErrors.LastRoleRemove) 
            : Result.Success();
    }
}