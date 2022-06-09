namespace Animato.Sso.Domain.Enums;

public class AuthorizationType : Enumeration
{
    /// <summary>
    /// User name and password
    /// </summary>
    public static readonly AuthorizationType Password = new(0, nameof(Password));

    /// <summary>
    /// TOTP - QRCode
    /// </summary>
    public static readonly AuthorizationType TotpQrCode = new(1, nameof(TotpQrCode));

    /// <summary>
    /// TOTP - SMS code
    /// </summary>
    public static readonly AuthorizationType TotpSms = new(2, nameof(TotpSms));

    /// <summary>
    /// Type of authorization
    /// </summary>
    /// <param name="value">Numeric value</param>
    /// <param name="name">Text description</param>
    public AuthorizationType(int value, string name) : base(value, name)
    {
    }

    public static AuthorizationType FromValue(int value) => FromValue<AuthorizationType>(value);
    public static AuthorizationType FromName(string name) => FromName<AuthorizationType>(name);
    public static IEnumerable<AuthorizationType> GetAll() => GetAll<AuthorizationType>();
}
