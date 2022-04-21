namespace Animato.Sso.Infrastructure.Services.Totp;
using System;
using Animato.Sso.Application.Common.Interfaces;
using Animato.Sso.Application.Models;
using Google.Authenticator;

public class GoogleQrCodeTotpAuthenticator : IQrCodeTotpAuthenticator
{
    private readonly GoogleQrCodeTotpAuthenticatorOptions options;
    private readonly TwoFactorAuthenticator authenticator;
    private readonly TimeSpan defaultClockDriftTolerance;

    public GoogleQrCodeTotpAuthenticator(GoogleQrCodeTotpAuthenticatorOptions options)
    {
        this.options = options ?? throw new ArgumentNullException(nameof(options));
        authenticator = new TwoFactorAuthenticator();
        defaultClockDriftTolerance = this.options.ToleranceInMinutes > 0
            ? TimeSpan.FromMinutes(this.options.ToleranceInMinutes)
            : TimeSpan.FromMinutes(5);
    }

    public QrCodeInfo GenerateCode(string account, string secretKey) => GenerateCode(account, secretKey, options.PixelsPerModule);

    public QrCodeInfo GenerateCode(string account, string secretKey, int pixelsPerModule)
    {
        var setupInfo = authenticator.GenerateSetupCode(options.Title, account, secretKey, false, 7);
        return new QrCodeInfo()
        {
            ImageUrl = setupInfo.QrCodeSetupImageUrl,
            ManualKey = setupInfo.ManualEntryKey
        };
    }

    public bool ValidatePin(string secretKey, string pin) => authenticator.ValidateTwoFactorPIN(secretKey, pin, defaultClockDriftTolerance);
}
