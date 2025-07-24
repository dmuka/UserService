using Domain.Users;

namespace Application.Abstractions.Email;

public interface IUrlGenerator
{
    string GenerateEmailConfirmationLink(Guid userId, string token);
}