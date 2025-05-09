using Core;

namespace Domain.Users.Specifications;

public class UserMustHaveAtLeastOneRecoveryCode(ICollection<string> recoveryCodes) : ISpecification
{
    public Result IsSatisfied()
    {
        if (recoveryCodes is null) return Result.Failure(Error.NullValue);
        
        return recoveryCodes.Count == 0 
            ? Result.Failure<ICollection<string>>(UserErrors.EmptyRecoveryCodesCollection) 
            : Result.Success();
    }
}