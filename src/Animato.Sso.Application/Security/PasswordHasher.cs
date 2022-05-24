namespace Animato.Sso.Application.Security;

using System.Security.Cryptography;
using Animato.Sso.Application.Common.Interfaces;

public class PasswordHasher : IPasswordHasher
{
    private const int PasswordBytesCount = 128;
    private const int IterationCount = 1000;

    public string GenerateSalt(int length = 16)
    {
        var saltBytes = new byte[length];
        RandomNumberGenerator.Fill(saltBytes);
        return Convert.ToBase64String(saltBytes);
    }
    public string HashPassword(string password, string salt)
    {
        var saltBytes = Convert.FromBase64String(salt);
        var key = new Rfc2898DeriveBytes(password, saltBytes, IterationCount, HashAlgorithmName.SHA256);
        return Convert.ToBase64String(key.GetBytes(PasswordBytesCount));
    }

    public bool IsValid(string passwordHash, string password, string salt)
        => passwordHash.Equals(HashPassword(password, salt), StringComparison.OrdinalIgnoreCase);
}
