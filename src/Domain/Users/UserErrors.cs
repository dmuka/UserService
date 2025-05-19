using Core;
using Domain.Users.Constants;

namespace Domain.Users;

public static class UserErrors
{
    public static Error NotFound(Guid userId) => Error.NotFound(
        Codes.NotFound, 
        $"The user with the id = '{userId}' was not found.");
    
    public static Error NotFoundByUsername(string userName) => Error.NotFound(
        Codes.NotFound, 
        $"The user with the user name = '{userName}' was not found.");
    
    public static Error NotFoundByEmail(string email) => Error.NotFound(
        Codes.NotFound, 
        $"The user with the email = '{email}' was not found.");
    
    public static Error EmailAlreadyConfirmed(string email) => Error.Failure(
        Codes.NotFound, 
        $"This email = '{email}' already confirmed.");
    
    public static Error WrongPassword() => Error.Failure(
        Codes.WrongPassword, 
        "You are entered the wrong password.");
    
    public static Error WrongRecoveryCode() => Error.Failure(
        Codes.WrongRecoveryCode, 
        "You are entered the wrong recovery code.");
    
    public static Error WrongVerificationCode() => Error.Failure(
        Codes.WrongRecoveryCode, 
        "You are entered the wrong verification code.");
    
    public static Error MfaModeEnabled() => Error.Failure(
        Codes.MfaModeEnabled, 
        "MFA mode is turned on: please enter verification or recovery code.");

    public static Error Unauthorized() => Error.Failure(
        Codes.Unauthorized,
        "You are not authorized to perform this action.");

    public static Error WrongResetCode() => Error.Failure(
        Codes.WrongResetCode,
        "The provided reset code is invalid.");

    public static readonly Error EmailAlreadyExists = Error.Conflict(
        Codes.EmailExists,
        "The provided email already exists.");

    public static readonly Error UsernameAlreadyExists = Error.Conflict(
        Codes.UsernameExists,
        "The provided username already exists.");

    public static readonly Error InvalidUsername = Error.Problem(
        Codes.InvalidUsername,
        "The provided username is invalid.");

    public static readonly Error InvalidUserId = Error.Problem(
        Codes.InvalidUserId,
        "The provided user id is invalid.");

    public static readonly Error InvalidFirstName = Error.Problem(
        Codes.InvalidFirstName,
        "The provided first name is invalid.");

    public static readonly Error InvalidLastName = Error.Problem(
        Codes.InvalidLastName,
        "The provided last name is invalid.");

    public static readonly Error InvalidMfaSecret = Error.Problem(
        Codes.InvalidMfaValue,
        "The provided MFA secret value is invalid.");

    public static readonly Error InvalidVerificationCode = Error.Problem(
        Codes.InvalidVerificationCode,
        "The provided verification code is invalid.");

    public static readonly Error InvalidMfaState = Error.Problem(
        Codes.InvalidMfaState,
        "MFA can't be enabled without providing a secret and recovery codes.");

    public static readonly Error UserEmailConfirmationError = Error.Problem(
        Codes.UserEmailConfirmationError,
        "User can't confirm his email.");

    public static readonly Error EmptyRecoveryCodesCollection = Error.Problem(
        Codes.EmptyRolesCollection,
        "User must have at least one recovery code if MFA is enabled.");

    public static readonly Error EmptyRolesCollection = Error.Problem(
        Codes.EmptyRolesCollection,
        "User must have at least one role.");

    public static readonly Error LastRoleRemove = Error.Problem(
        Codes.LastRoleRemove,
        "User must have at least one role after remove role.");
}