using Core;
using Domain.Users.Constants;

namespace Domain.Users;

public static class UserErrors
{
    public static Error NotFound(ulong userId) => Error.NotFound(
        Codes.NotFound, 
        $"The user with the id = '{userId}' was not found.");

    public static Error Unauthorized() => Error.Failure(
        "Users.Unauthorized",
        "You are not authorized to perform this action.");
}