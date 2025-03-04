using Core;

namespace Domain.Users.Specifications;

public class MustBeNonNull<T>(Result<T> result)
 {
     public Result IsSatisfied()
     {
         if (result.IsFailure) return result;
         
         return result.Value is null ? Result.Failure<T>(Error.NullValue) : Result.Success();
     }
 }