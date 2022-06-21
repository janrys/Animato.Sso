namespace Animato.Sso.Application.Features.Users.DTOs;
using Animato.Sso.Application.Common;
using Animato.Sso.Application.Common.Interfaces;

public static class UserExensions
{
    public static bool IsSystemUser(this Domain.Entities.User user)
        => user.Id.Equals(Domain.Entities.User.SystemUser) || user.Id.Equals(Domain.Entities.User.EmptyUser);

    public static Domain.Entities.User ApplyTo(this CreateUserModel model, Domain.Entities.User user)
    {
        if (!string.IsNullOrEmpty(model.Login))
        {
            user.Login = model.Login;
        }

        if (!string.IsNullOrEmpty(model.FullName))
        {
            user.FullName = model.FullName;
        }

        if (!string.IsNullOrEmpty(model.Name))
        {
            user.Name = model.Name;
        }

        if (!string.IsNullOrEmpty(model.Password))
        {
            user.Password = model.Password;
        }

        if (!string.IsNullOrEmpty(model.TotpSecretKey))
        {
            user.TotpSecretKey = model.TotpSecretKey;
        }

        if (model.AuthorizationMethod is not null)
        {
            user.AuthorizationMethod = model.AuthorizationMethod;
        }

        user.IsDeleted = model.IsDeleted ?? user.IsDeleted;
        user.IsBlocked = model.IsBlocked ?? user.IsBlocked;
        return user;
    }

    public static CreateUserModel ValidateAndSanitize(this CreateUserModel user
        , OidcOptions oidcOptions
        , ITokenFactory tokenFactory
        , IPasswordFactory passwordFactory)
    {
        if (string.IsNullOrEmpty(user.Login))
        {
            throw new Exceptions.ValidationException(
                Exceptions.ValidationException.CreateFailure(nameof(user.Login)
                , $"User login must have a value"));
        }

        if (string.IsNullOrEmpty(user.Password))
        {
            user.Password = tokenFactory.GenerateRandomString(oidcOptions.DefaultUserPasswordLength);
        }
        else
        {
            var passwordStrength = passwordFactory.CheckPasswordStrength(user.Password);
            if (passwordStrength.Strength.Value <= oidcOptions.MinimalPasswordStrength)
            {
                throw new Exceptions.ValidationException(
                Exceptions.ValidationException.CreateFailure(nameof(user.Password)
                , $"Password is too weak, score {passwordStrength.Strength.Value} - {passwordStrength.Strength.Name}. Reason: {passwordStrength.Warning}"));
            }
        }

        if (string.IsNullOrEmpty(user.TotpSecretKey))
        {
            user.TotpSecretKey = tokenFactory.GenerateRandomString(oidcOptions.TotpSecretLength);
        }

        if (string.IsNullOrEmpty(user.FullName))
        {
            user.FullName = user.Login;
        }

        if (user.AuthorizationMethod is null)
        {
            user.AuthorizationMethod = Domain.Enums.AuthorizationMethod.Password;
        }

        if (!user.IsDeleted.HasValue)
        {
            user.IsDeleted = false;
        }

        if (!user.IsBlocked.HasValue)
        {
            user.IsBlocked = false;
        }

        return user;
    }
}
