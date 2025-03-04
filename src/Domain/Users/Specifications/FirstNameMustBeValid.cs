using Core;

namespace Domain.Users.Specifications;

public class FirstNameMustBeValid(string firstName) : ISpecification
{
    private const int MinLength = 3;
    private const int MaxLength = 100;
    
    public Result IsSatisfied()
    {
        if (string.IsNullOrEmpty(firstName) 
               || firstName.Length < MinLength 
               || firstName.Length > MaxLength) return Result.Failure<string>(UserErrors.InvalidFirstName);
        
        return Result.Success();
    }
}