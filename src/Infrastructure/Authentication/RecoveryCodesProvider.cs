using System.Security.Cryptography;
using Application.Abstractions.Authentication;

namespace Infrastructure.Authentication;

internal sealed class RecoveryCodesProvider(PasswordHasher hasher) : IRecoveryCodesProvider
{
    public List<(string code, string hashCode)> GenerateRecoveryCodes(int count = 8)
    {
        var codes = new List<(string code, string hashCode)>(count);

        for (var i = 0; i < count; i++)
        {
            var codeBytes = RandomNumberGenerator.GetBytes(12);
            var code = Convert.ToBase64String(codeBytes)[..16].Replace("/", "-").Replace("+", "-");
            code = $"{code[..4]}-{code.Substring(4, 4)}-{code.Substring(8, 4)}-{code.Substring(12, 4)}";

            var hashedCode = hasher.GetHash(code);

            codes.Add((code, hashedCode));
        }

        return codes;
    }

    public bool VerifyRecoveryCode(string code, string hashedCode)
    {
        return hasher.CheckPassword(hashedCode, code);
    }
}