namespace Animato.Sso.Application.Models;
using Animato.Sso.Domain.Enums;

public class LogoType : Enumeration
{
    /// <summary>
    /// Default brand for new partners
    /// </summary>
    public static readonly LogoType Application = new(0, nameof(Application));

    /// <summary>
    /// Inherited brand from other partner
    /// </summary>
    public static readonly LogoType Company = new(1, nameof(Company));

    /// <summary>
    /// Custom defined brand
    /// </summary>
    public static readonly LogoType Output = new(2, nameof(Output));

    public LogoType(int value, string name) : base(value, name)
    {
    }

    public static LogoType FromValue(int value) => FromValue<LogoType>(value);
    public static LogoType FromName(string name) => FromName<LogoType>(name);
    public static IEnumerable<LogoType> GetAll() => GetAll<LogoType>();
}
