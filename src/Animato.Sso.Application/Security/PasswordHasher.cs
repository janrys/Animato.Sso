namespace Animato.Sso.Application.Security;

using System.Security.Cryptography;
using Animato.Sso.Application.Common.Interfaces;
using Animato.Sso.Domain.Enums;

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
