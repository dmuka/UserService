using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Options.Email;

public record GmailOptions
{
    [Required, MinLength(2)]
    public required string SmtpHost { get; set; }
    [Required, Range(1, 99999)]
    public required int SmtpPort { get; set; }
    [Required, EmailAddress]
    public required string FromEmail { get; set; } 
    [Required, MinLength(4)]
    public required string UserName { get; set; }
    [Required, MinLength(5)]
    public required string Password { get; set; }
}