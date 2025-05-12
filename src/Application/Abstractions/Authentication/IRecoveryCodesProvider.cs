namespace Application.Abstractions.Authentication;

public interface IRecoveryCodesProvider
{
    List<(string code, string hashCode)> GenerateRecoveryCodes(int count = 8);
    bool VerifyRecoveryCode(string code, string hashedCode);
}