namespace Animato.Sso.Domain.Enums;

public class PermissionType : Enumeration
{
    /// <summary>
    /// Default brand for new partners
    /// </summary>
    public static readonly PermissionType Partner = new(0, nameof(Partner));

    /// <summary>
    /// Inherited brand from other partner
    /// </summary>
    public static readonly PermissionType PartnerRole = new(0, nameof(PartnerRole));

    /// <summary>
    /// Type of partner permission. Values Partner = 0, PartnerRole = 1
    /// </summary>
    /// <param name="value">Numeric value</param>
    /// <param name="name">Text description</param>
    public PermissionType(int value, string name) : base(value, name)
    {
    }

    public static PermissionType FromValue(int value) => FromValue<PermissionType>(value);
    public static PermissionType FromName(string name) => FromName<PermissionType>(name);
    public static IEnumerable<PermissionType> GetAll() => GetAll<PermissionType>();
}
