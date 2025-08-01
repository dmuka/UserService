﻿using System.Web;
using Application.Abstractions.Email;
using Domain.Users;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Email;

public class UrlGenerator(IConfiguration configuration) : IUrlGenerator
{
    public string GenerateEmailConfirmationLink(Guid userId, string token)
    {
        var baseUrl = configuration["App:BaseUrl"] ?? "https://localhost:5001";
        baseUrl = baseUrl.TrimEnd('/');
        
        return $"{baseUrl}/Account/ConfirmEmail?userId={userId}&token={HttpUtility.UrlEncode(token)}";
    }
}