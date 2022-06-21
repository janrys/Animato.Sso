namespace Animato.Sso.Application.Security;

using Ardalis.SmartEnum;

public sealed class PasswordStrength : SmartEnum<PasswordStrength>
{
    /// <summary>
    /// 0 # too guessable: risky password. (guesses &lt; 10^3)
    /// </summary>
    public static readonly PasswordStrength Poor = new(0, nameof(Poor));

    /// <summary>
    /// 1 # very guessable: protection from throttled online attacks. (guesses &lt; 10^6)
    /// </summary>
    public static readonly PasswordStrength Weak = new(1, nameof(Weak));

    /// <summary>
    /// 2 # somewhat guessable: protection from unthrottled online attacks. (guesses &lt; 10^8)
    /// </summary>
    public static readonly PasswordStrength Moderate = new(2, nameof(Moderate));

    /// <summary>
    /// 3 # safely unguessable: moderate protection from offline slow-hash scenario. (guesses &lt; 10^10)
    /// </summary>
    public static readonly PasswordStrength Good = new(3, nameof(Good));

    /// <summary>
    /// 4 # very unguessable: strong protection from offline slow-hash scenario. (guesses >= 10^10)
    /// </summary>
    public static readonly PasswordStrength Strong = new(4, nameof(Strong));

    /// <summary>
    /// Type of brand. Values Default = 0, Inherited = 1, Custom = 2
    /// </summary>
    /// <param name="value">Numeric value</param>
    /// <param name="name">Text description</param>
    public PasswordStrength(int value, string name) : base(name, value) { }
}


