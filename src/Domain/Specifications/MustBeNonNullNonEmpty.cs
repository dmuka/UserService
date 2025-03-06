using Core;
using Domain.ValueObjects.Emails;

namespace Domain.Specifications;

public class MustBeNonNullNonEmpty(string value)
{
    public Result IsSatisfied()
    {
        if (value is null) return Result.Failure<string>(Error.NullValue);
        if (value == string.Empty) return Result.Failure<string>(Error.EmptyValue);
        
        return Result.Success(value);
    }
}