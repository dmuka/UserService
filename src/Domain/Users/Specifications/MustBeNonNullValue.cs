using Core;

namespace Domain.Users.Specifications;

public class MustBeNonNullValue<T>(T value)
 {
     public Result IsSatisfied()
     {
         return value is null 
             ? Result.Failure<T>(Error.NullValue) 
             : Result.Success<T>(value);
     }
 }