using Domain.Users.Constants;

namespace Domain.Users;

public static class UserErrors
{
    public static Error NotFound(int userId) => Error.NotFound(Codes.NotFound, $"The user with the id = '{userId}' was not found.");
}