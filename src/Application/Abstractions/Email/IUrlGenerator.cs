using Domain.Users;

namespace Application.Abstractions.Email;

public interface IUrlGenerator
{
    string GenerateEmailConfirmationLink(UserId userId, string token);
}