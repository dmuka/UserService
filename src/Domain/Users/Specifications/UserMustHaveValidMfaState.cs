using Core;

namespace Domain.Users.Specifications;

public class UserMustHaveValidMfaState(
    string? mfaSecret,
    bool isMfaEnabled,
    ICollection<string>? recoveryCodes) : ISpecification
 {
     public Result IsSatisfied()
     {
         return (isMfaEnabled && mfaSecret != null && recoveryCodes?.Count != 0) || !isMfaEnabled
             ? Result.Success()
             : Result.Failure(UserErrors.InvalidMfaState);
     }
 }