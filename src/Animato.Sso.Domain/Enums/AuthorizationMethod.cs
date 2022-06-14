namespace Animato.Sso.Domain.Enums;

public class AuthorizationMethod : Enumeration
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
    public AuthorizationMethod(int value, string name) : base(value, name)
    {
    }

    public static AuthorizationMethod FromValue(int value) => FromValue<AuthorizationMethod>(value);
    public static AuthorizationMethod FromName(string name) => FromName<AuthorizationMethod>(name);
    public static IEnumerable<AuthorizationMethod> GetAll() => GetAll<AuthorizationMethod>();
}
