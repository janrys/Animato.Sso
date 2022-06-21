namespace Animato.Sso.Domain.Enums;

using Ardalis.SmartEnum;

public sealed class AuthorizationMethod : SmartEnum<AuthorizationMethod>
{
    /// <summary>
    /// User name and password
    /// </summary>
    public static readonly AuthorizationMethod Unknown = new(0, nameof(Unknown));

    /// <summary>
    /// User name and password
    /// </summary>
    public static readonly AuthorizationMethod Password = new(1, nameof(Password));

    /// <summary>
    /// TOTP - QRCode
    /// </summary>
    public static readonly AuthorizationMethod TotpQrCode = new(2, nameof(TotpQrCode));

    /// <summary>
    /// TOTP - SMS code
    /// </summary>
    public static readonly AuthorizationMethod TotpSms = new(3, nameof(TotpSms));

    /// <summary>
    /// Type of authorization
    /// </summary>
    /// <param name="value">Numeric value</param>
    /// <param name="name">Text description</param>
    public AuthorizationMethod(int value, string name) : base(name, value)
    {
    }
}
