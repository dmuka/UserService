using Core;
using Domain.ValueObjects.Emails;

namespace Domain.Users.Specifications;

public class EmailMustBeValid(string email)
{
    public Result IsSatisfied()
    {
        if (email is null) return Result.Failure<Email>(Error.NullValue);
        if (email == string.Empty) return Result.Failure<Email>(Error.EmptyValue);
        
        var result = Email.Create(email);
        
        return result.IsFailure ? Result.Failure<Email>(EmailErrors.InvalidEmail) : Result.Success();
    }
}