namespace Application.Abstractions.Authentication;

public interface IPasswordHasher
{
    string GetHash(string password);

    bool CheckPassword(string password, string passwordHash);
}
