using Application.Abstractions.Authentication;
using Microsoft.Extensions.Logging;
using OtpNet;
using QRCoder;

namespace Infrastructure.Authentication;

public class TotpProvider(ILogger<TotpProvider> logger) : ITotpProvider
{
    public string GenerateSecretKey()
    {
        var key = KeyGeneration.GenerateRandomKey(20);
        var base32Secret = Base32Encoding.ToString(key);
        
        return base32Secret;
    }

    public string GetQr(string secretKey, string userEmail, string issuer)
    {
        var provisioningUrl = new OtpUri(OtpType.Totp, secretKey, userEmail);
        
        var qrGenerator = new QRCodeGenerator();
        var qrCodeData = qrGenerator.CreateQrCode(provisioningUrl.ToString(), QRCodeGenerator.ECCLevel.Q);
        var qrCode = new PngByteQRCode(qrCodeData);
        var qrCodeImage = qrCode.GetGraphic(20);
        var qr = Convert.ToBase64String(qrCodeImage);
        
        return qr;
    }

    public bool ValidateTotp(string secretKey, int code)
    {
        var totp = new Totp(Base32Encoding.ToBytes(secretKey));
        var verificationResult = totp.VerifyTotp(code.ToString(), out var timeWindowUsed, VerificationWindow.RfcSpecifiedNetworkDelay);

        logger.LogInformation("TOTP validation used time window: {windows}", timeWindowUsed);
        
        return verificationResult;
    }
}