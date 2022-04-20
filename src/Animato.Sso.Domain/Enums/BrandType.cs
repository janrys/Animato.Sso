namespace Animato.Sso.Domain.Enums;

public class BrandType : Enumeration
{
    /// <summary>
    /// Default brand for new partners
    /// </summary>
    public static readonly BrandType Default = new(0, nameof(Default));

    /// <summary>
    /// Inherited brand from other partner
    /// </summary>
    public static readonly BrandType Inherited = new(1, nameof(Inherited));

    /// <summary>
    /// Custom defined brand
    /// </summary>
    public static readonly BrandType Custom = new(2, nameof(Custom));

    /// <summary>
    /// Type of brand. Values Default = 0, Inherited = 1, Custom = 2
    /// </summary>
    /// <param name="value">Numeric value</param>
    /// <param name="name">Text description</param>
    public BrandType(int value, string name) : base(value, name)
    {
    }

    public static BrandType FromValue(int value) => FromValue<BrandType>(value);
    public static BrandType FromName(string name) => FromName<BrandType>(name);
    public static IEnumerable<BrandType> GetAll() => GetAll<BrandType>();
}
