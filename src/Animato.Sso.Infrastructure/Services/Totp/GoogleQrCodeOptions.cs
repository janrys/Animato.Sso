namespace Animato.Sso.Infrastructure.Services.Totp;

public class GoogleQrCodeTotpAuthenticatorOptions
{
    public const string CONFIGURATION_KEY = "GoogleQrCodeTotp";
    public string Title { get; set; } = "Animato";
    public int PixelsPerModule { get; set; } = 10;
    public int ToleranceInMinutes { get; set; } = 5;
}
