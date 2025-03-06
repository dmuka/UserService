using Core;
using Domain.RefreshTokens.Constants;

namespace Domain.RefreshTokens;

public static class RefreshTokenErrors
{
    public static Error NotFound(Guid refreshTokenId) => Error.NotFound(
        Codes.NotFound, 
        $"The refresh token with the id = '{refreshTokenId}' was not found.");
    
    public static Error NotFoundByValue(string value) => Error.NotFound(
        Codes.NotFound, 
        $"The refresh token with the value = '{value}' was not found.");

    public static Error Unauthorized() => Error.Failure(
        Codes.Unauthorized,
        "You are not authorized to perform this action.");

    public static readonly Error InvalidExpiresDate = Error.Problem(
        Codes.InvalidExpiresDate,
        "The provided expire date can't be in the past.");
}