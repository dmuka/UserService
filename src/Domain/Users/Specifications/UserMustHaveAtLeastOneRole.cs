using Core;
using Domain.ValueObjects.RoleNames;

namespace Domain.Users.Specifications;

public class UserMustHaveAtLeastOneRole(ICollection<RoleName> roleNames) : ISpecification
{
    public Result IsSatisfied()
    {
        if (roleNames is null) return Result.Failure(Error.NullValue);
        
        return roleNames.Count == 0 
            ? Result.Failure<ICollection<RoleName>>(UserErrors.EmptyRolesCollection) 
            : Result.Success();
    }
}