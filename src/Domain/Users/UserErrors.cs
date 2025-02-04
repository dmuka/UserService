using Core;
using Domain.Users.Constants;

namespace Domain.Users;

public static class UserErrors
{
    public static Error NotFound(Guid userId) => Error.NotFound(
        Codes.NotFound, 
        $"The user with the id = '{userId}' was not found.");
    
    public static Error NotFound(string userName) => Error.NotFound(
        Codes.NotFound, 
        $"The user with the user name = '{userName}' was not found.");

    public static Error Unauthorized() => Error.Failure(
        Codes.Unauthorized,
        "You are not authorized to perform this action.");

    public static readonly Error EmailAlreadyExists = Error.Conflict(
        Codes.EmailExists,
        "The provided email already exists.");

    public static readonly Error UsernameAlreadyExists = Error.Conflict(
        Codes.UsernameExists,
        "The provided username already exists.");
}