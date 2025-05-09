namespace Application.Abstractions.Authentication;

public interface ITotpProvider
{
    string GenerateSecretKey();

    string GetQr(string secretKey, string userEmail, string issuer);

    bool ValidateTotp(string secretKey, int code);
}