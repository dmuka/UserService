namespace Domain.Users.Constants;

public static class Codes
{
    public const string NotFound = "UserNotFound";
    public const string Unauthorized = "UserUnauthorized";
    public const string WrongPassword = "UserWrongPassword";
    public const string WrongRecoveryCode = "WrongRecoveryCode";
    public const string WrongVerificationCode = "WrongVerificationCode";
    public const string MfaModeEnabled = "MfaModeEnabled";
    public const string WrongResetCode = "UserWrongResetCode";
    public const string EmailExists = "EmailAlreadyExists";
    public const string EmailConfirmed = "EmailAlreadyConfirmed";
    public const string UsernameExists = "UsernameAlreadyExists";
    public const string InvalidUsername = "InvalidUsername";
    public const string InvalidUserId = "InvalidUserId";
    public const string InvalidFirstName = "InvalidFirstName";
    public const string InvalidLastName = "InvalidLastName";
    public const string InvalidMfaValue = "InvalidMfa";
    public const string InvalidVerificationCode = "InvalidVerificationCode";
    public const string InvalidMfaState = "InvalidMfaState";
    public const string UserEmailConfirmationError = "UserEmailConfirmationError";
    public const string UserEmailNotConfirmedYet = "UserEmailNotConfirmedYet";
    public const string EmptyRolesCollection = "EmptyRolesCollection";
    public const string EmptyRecoveryCodesCollection = "EmptyRecoveryCodesCollection";
    public const string LastRoleRemove = "LastRoleRemove";
}