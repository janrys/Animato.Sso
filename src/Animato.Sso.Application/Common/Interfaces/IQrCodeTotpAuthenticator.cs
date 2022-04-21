namespace Animato.Sso.Application.Common.Interfaces;

using Animato.Sso.Application.Models;

public interface IQrCodeTotpAuthenticator
{
    QrCodeInfo GenerateCode(string account, string secretKey);
    QrCodeInfo GenerateCode(string account, string secretKey, int pixelsPerModule);
    bool ValidatePin(string secretKey, string pin);
}
