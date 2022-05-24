namespace Animato.Sso.Domain.Enums;

public class GrantType : Enumeration
{
    /// <summary>
    /// Default brand for new partners
    /// </summary>
    public static readonly GrantType Code = new(0, nameof(Code), "authorization_code");



    public string GrantCode { get; }

    /// <summary>
    /// Type of brand. Values Default = 0, Inherited = 1, Custom = 2
    /// </summary>
    /// <param name="value">Numeric value</param>
    /// <param name="name">Text description</param>
    /// <param name="grantCode">Code used in grant request</param>
    public GrantType(int value, string name, string grantCode) : base(value, name)
        => GrantCode = grantCode;

    public static GrantType FromValue(int value) => FromValue<GrantType>(value);
    public static GrantType FromName(string name) => FromName<GrantType>(name);
    public static IEnumerable<GrantType> GetAll() => GetAll<GrantType>();
}
