using Core;
using Domain.ValueObjects.Constants;

namespace Domain.ValueObjects.Emails;

public static class EmailErrors
{
    public static readonly Error InvalidEmail = Error.Problem(
        Codes.InvalidEmail,
        "The provided email is invalid.");
}