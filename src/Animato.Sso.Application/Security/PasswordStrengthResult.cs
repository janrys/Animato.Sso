namespace Animato.Sso.Application.Security;

public class PasswordStrengthResult
{
    public PasswordStrengthResult(PasswordStrength passwordStrength, string warning)
    {
        Strength = passwordStrength;
        Warning = warning;
    }

    public PasswordStrength Strength { get; private set; }
    public string Warning { get; }
}


