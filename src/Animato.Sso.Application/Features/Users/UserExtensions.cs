namespace Animato.Sso.Application.Features.Users;
using Animato.Sso.Application.Common.Interfaces;
using Animato.Sso.Domain.Entities;

public static class UserExtensions
{
    public static User UpdatePasswordAndHash(this User user, IPasswordFactory passwordHasher, IDateTimeService dateTime)
     => user.UpdatePasswordAndHash(passwordHasher, user.Password, dateTime);

    public static User UpdatePasswordAndHash(this User user, IPasswordFactory passwordHasher, string password, IDateTimeService dateTime)
    {
        user.Salt = passwordHasher.GenerateSalt();

        if (user.PasswordHashAlgorithm is null)
        {
            user.PasswordHashAlgorithm = Domain.Enums.HashAlgorithmType.SHA256;
        }

        user.Password = passwordHasher.HashPassword(password, user.Salt, user.PasswordHashAlgorithm);
        user.PasswordLastChanged = dateTime.UtcNow;
        return user;
    }
}
