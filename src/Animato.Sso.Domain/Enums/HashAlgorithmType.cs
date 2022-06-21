namespace Animato.Sso.Domain.Enums;

using Ardalis.SmartEnum;

public sealed class HashAlgorithmType : SmartEnum<HashAlgorithmType>
{
    /// <summary>
    /// SHA256
    /// </summary>
    public static readonly HashAlgorithmType SHA256 = new(0, nameof(SHA256));

    /// <summary>
    /// SHA512
    /// </summary>
    public static readonly HashAlgorithmType SHA512 = new(1, nameof(SHA512));


    /// <summary>
    /// Type of hash algorithm
    /// </summary>
    /// <param name="value">Numeric value</param>
    /// <param name="name">Text description</param>
    public HashAlgorithmType(int value, string name) : base(name, value) { }
}
