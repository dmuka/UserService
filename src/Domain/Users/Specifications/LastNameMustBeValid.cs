using Core;

namespace Domain.Users.Specifications;

public class LastNameMustBeValid(string lastName) : ISpecification
{
    private const int MinLength = 3;
    private const int MaxLength = 100;
    
    public Result IsSatisfied()
    {
        if (string.IsNullOrEmpty(lastName) 
               || lastName.Length < MinLength 
               || lastName.Length > MaxLength) return Result.Failure<string>(UserErrors.InvalidLastName);
        
        return Result.Success();
    }
}