using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Options.Email;

public record SmtpOptions
{
    [Required, MinLength(2)]
    public required string SmtpHost { get; set; }
    [Required, Range(1, 99999)]
    public required int SmtpPort { get; set; }
    [Required, EmailAddress]
    public required string FromEmail { get; set; } 
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}