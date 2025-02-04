using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Options.Authentication;

public class AuthOptions
{
    [Required, MinLength(15)]
    public required string Secret { get; set; }
    [Required, MinLength(2)]
    public required string Issuer { get; set; } 
    [Required, MinLength(4)]
    public required string Audience { get; set; }
    [Required, Range(0, 100)]
    public required int ExpirationInMinutes { get; set; }
}