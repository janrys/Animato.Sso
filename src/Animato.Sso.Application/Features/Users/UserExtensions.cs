namespace Animato.Sso.Application.Features.Users;
using Animato.Sso.Application.Common.Interfaces;
using Animato.Sso.Domain.Entities;

public static class UserExtensions
{
    public static User UpdatePasswordAndHash(this User user, IPasswordHasher passwordHasher)
     => user.UpdatePasswordAndHash(passwordHasher, user.Password);

    public static User UpdatePasswordAndHash(this User user, IPasswordHasher passwordHasher, string password)
    {
        user.Salt = passwordHasher.GenerateSalt();
        user.Password = passwordHasher.HashPassword(password, user.Salt);
        return user;
    }
}
