namespace Animato.Sso.Domain.Enums;

public class TokenType : Enumeration
{
    /// <summary>
    /// Access token
    /// </summary>
    public static readonly TokenType Access = new(0, nameof(Access));

    /// <summary>
    /// Refresh token
    /// </summary>
    public static readonly TokenType Refresh = new(1, nameof(Refresh));

    /// <summary>
    /// Id token
    /// </summary>
    public static readonly TokenType Id = new(2, nameof(Id));

    /// <summary>
    /// Type of brand. Values Default = 0, Inherited = 1, Custom = 2
    /// </summary>
    /// <param name="value">Numeric value</param>
    /// <param name="name">Text description</param>
    public TokenType(int value, string name) : base(value, name)
    {
    }

    public static TokenType FromValue(int value) => FromValue<TokenType>(value);
    public static TokenType FromName(string name) => FromName<TokenType>(name);
    public static IEnumerable<TokenType> GetAll() => GetAll<TokenType>();
}
