namespace Animato.Sso.Application.Common.Interfaces;
public interface IPasswordHasher
{
    string GenerateSalt(int length = 16);
    string HashPassword(string password, string salt);
    bool IsValid(string passwordHash, string password, string salt);
}
