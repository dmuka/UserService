using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Options.Authentication;

public record AuthOptions
{
    [Required, MinLength(32)]
    public required string Secret { get; set; }
    [Required, MinLength(2)]
    public required string Issuer { get; set; } 
    [Required, MinLength(4)]
    public required string Audience { get; set; }
    [Required, Range(0, 15)]
    public required int AccessTokenExpirationInMinutes { get; set; }
    [Required, Range(0, 30)]
    public required int AccessTokenCookieExpirationInMinutes { get; set; }
    [Required, Range(0, 24)]
    public required int SessionIdCookieExpirationInHours { get; set; }
    [Required, Range(0, 30)]
    public required int RefreshTokenExpirationInDays { get; set; }
    [Required, Range(0, 30)]
    public required int ResetPasswordTokenExpirationInMinutes { get; set; }
}