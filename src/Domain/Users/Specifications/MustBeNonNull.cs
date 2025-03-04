using Core;

namespace Domain.Users.Specifications;

public class MustBeNonNull<T>(T value)
{
    public Result IsSatisfied()
    {
        return value is null ? Result.Failure<T>(Error.NullValue) : Result.Success();
    }
}