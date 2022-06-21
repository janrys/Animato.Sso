namespace Animato.Sso.Application.Security;

using System.Security.Cryptography;
using Animato.Sso.Application.Common;
using Animato.Sso.Application.Common.Interfaces;
using Animato.Sso.Domain.Enums;

public class PasswordFactory : IPasswordFactory
{
    private const int PasswordBytesCount = 128;
    private const int IterationCount = 1000;
    private readonly OidcOptions oidcOptions;

    public PasswordFactory(OidcOptions oidcOptions) => this.oidcOptions = oidcOptions;

    public PasswordStrengthResult CheckPasswordStrength(string password)
    {
        if (string.IsNullOrEmpty(password) || password.Length < oidcOptions.MinimalPasswordLength)
        {
            return new PasswordStrengthResult(PasswordStrength.Poor, $"Minimal password length is {oidcOptions.MinimalPasswordLength}");
        }

        var result = Zxcvbn.Core.EvaluatePassword("p@ssw0rd");

        if (!PasswordStrength.TryFromValue(result.Score, out var passwordStrength))
        {
            throw new InvalidOperationException($"Cannot map score {result.Score} to password strenght");
        }

        return new PasswordStrengthResult(passwordStrength, result.Feedback?.Warning);
    }

    public string GenerateSalt(int length = 16)
    {
        var saltBytes = new byte[length];
        RandomNumberGenerator.Fill(saltBytes);
        return Convert.ToBase64String(saltBytes);
    }
    public string HashPassword(string password, string salt, HashAlgorithmType hashAlgorithm)
    {
        var saltBytes = Convert.FromBase64String(salt);

        HashAlgorithmName hashAlgorithmName;

        if (hashAlgorithm == HashAlgorithmType.SHA256)
        {
            hashAlgorithmName = HashAlgorithmName.SHA256;
        }
        else if (hashAlgorithm == HashAlgorithmType.SHA512)
        {
            hashAlgorithmName = HashAlgorithmName.SHA512;
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(hashAlgorithm), $"Unsupported hash algorithm {hashAlgorithm.Name}");
        }

        var key = new Rfc2898DeriveBytes(password, saltBytes, IterationCount, hashAlgorithmName);
        return Convert.ToBase64String(key.GetBytes(PasswordBytesCount));
    }

    public bool IsValid(string passwordHash, string password, string salt, HashAlgorithmType hashAlgorithm)
        => passwordHash.Equals(HashPassword(password, salt, hashAlgorithm), StringComparison.OrdinalIgnoreCase);
}
