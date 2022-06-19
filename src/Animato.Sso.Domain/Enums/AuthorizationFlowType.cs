namespace Animato.Sso.Domain.Enums;

public class AuthorizationFlowType : Enumeration
{
    /// <summary>
    /// Authorization code flow
    /// </summary>
    public static readonly AuthorizationFlowType Code = new(0, nameof(Code), "code");

    /// <summary>
    /// Implicit grant flow
    /// </summary>
    public static readonly AuthorizationFlowType Token = new(1, nameof(Token), "token");

    /// <summary>
    /// OIDC flows with id token
    /// </summary>
    public static readonly AuthorizationFlowType IdToken = new(2, nameof(IdToken), "id_token");

    public string RequestCode { get; }

    /// <summary>
    /// Type of brand. Values Default = 0, Inherited = 1, Custom = 2
    /// </summary>
    /// <param name="value">Numeric value</param>
    /// <param name="name">Text description</param>
    /// <param name="requestCode">Code used in authorization request</param>
    public AuthorizationFlowType(int value, string name, string requestCode) : base(value, name)
        => RequestCode = requestCode;

    public static AuthorizationFlowType FromValue(int value) => FromValue<AuthorizationFlowType>(value);
    public static AuthorizationFlowType FromName(string name) => FromName<AuthorizationFlowType>(name);
    public static IEnumerable<AuthorizationFlowType> GetAll() => GetAll<AuthorizationFlowType>();
}
