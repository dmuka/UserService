namespace Domain.Users.Constants;

public static class Codes
{
    public const string NotFound = "UserNotFound";
    public const string Unauthorized = "UserUnauthorized";
    public const string WrongPassword = "UserWrongPassword";
    public const string WrongResetCode = "UserWrongResetCode";
    public const string EmailExists = "EmailAlreadyExists";
    public const string UsernameExists = "UsernameAlreadyExists";
    public const string InvalidUsername = "InvalidUsername";
    public const string InvalidFirstName = "InvalidFirstName";
    public const string InvalidLastName = "InvalidLastName";
    public const string EmptyRolesCollection = "EmptyRolesCollection";
    public const string LastRoleRemove = "LastRoleRemove";
}