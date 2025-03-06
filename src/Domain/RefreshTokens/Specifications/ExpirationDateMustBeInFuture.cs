using Core;
using Domain.ValueObjects.Emails;

namespace Domain.RefreshTokens.Specifications;

public class ExpirationDateMustBeInFuture(DateTime expiresUtc)
{
    public Result IsSatisfied()
    {
        return expiresUtc <= DateTime.UtcNow 
            ? Result.Failure<RefreshToken>(RefreshTokenErrors.InvalidExpiresDate) 
            : Result.Success();
    }
}