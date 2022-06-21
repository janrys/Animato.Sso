namespace Animato.Sso.Application.Common.Interfaces;

using Animato.Sso.Domain.Enums;

public interface IPasswordHasher
{
    string GenerateSalt(int length = 16);
    string HashPassword(string password, string salt, HashAlgorithmType hashAlgorithm);
    bool IsValid(string passwordHash, string password, string salt, HashAlgorithmType hashAlgorithm);
}
