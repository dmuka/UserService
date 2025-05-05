using Core;

namespace Domain.Roles.Specifications;

public class RoleNameMustBeValid(string roleName) : ISpecification
{
    private const int MinLength = 3;
    private const int MaxLength = 50;
    
    public Result IsSatisfied()
    {
        if (string.IsNullOrEmpty(roleName) 
               || roleName.Length < MinLength 
               || roleName.Length > MaxLength) return Result.Failure<string>(RoleErrors.InvalidRoleName);
        
        return Result.Success();
    }
}