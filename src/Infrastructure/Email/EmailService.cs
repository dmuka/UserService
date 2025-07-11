﻿using System.Net;
using System.Net.Mail;
using Application.Abstractions.Email;
using Infrastructure.Options.Email;
using Microsoft.Extensions.Options;

namespace Infrastructure.Email;

public class EmailService(IOptions<SmtpOptions> options) : IEmailService
{
    private readonly SmtpClient _smtpClient = new(options.Value.SmtpHost, options.Value.SmtpPort)
    {
        Credentials = new NetworkCredential(options.Value.UserName, options.Value.Password),
        EnableSsl = true 
    };

    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        var mailMessage = new MailMessage(options.Value.FromEmail, toEmail, subject, body)
        {
            IsBodyHtml = true
        };
        
        await _smtpClient.SendMailAsync(mailMessage);
    }
}