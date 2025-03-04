using Core;

namespace Domain.Users.Specifications;

public class UserNameMustBeValid(string userName) : ISpecification
{
    private const int MinLength = 3;
    private const int MaxLength = 50;
    
    public Result IsSatisfied()
    {
        if (string.IsNullOrEmpty(userName) 
               || userName.Length < MinLength 
               || userName.Length > MaxLength) return Result.Failure<string>(UserErrors.InvalidUsername);
        
        return Result.Success();
    }
}