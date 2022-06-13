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

        if (model.AuthorizationType is not null)
        {
            user.AuthorizationType = model.AuthorizationType;
        }

        user.IsDeleted = model.IsDeleted ?? user.IsDeleted;
        user.IsBlocked = model.IsBlocked ?? user.IsBlocked;
        return user;
    }

    public static CreateUserModel ValidateAndSanitize(this CreateUserModel user
        , OidcOptions oidcOptions
        , ITokenFactory tokenFactory)
    {
        if (string.IsNullOrEmpty(user.Login))
        {
            throw new Exceptions.ValidationException(
                Exceptions.ValidationException.CreateFailure(nameof(user.Login)
                , $"User login must have a value"));
        }

        if (string.IsNullOrEmpty(user.Password))
        {
            user.Password = tokenFactory.GenerateRandomString(oidcOptions.UserPasswordLength);
        }

        if (string.IsNullOrEmpty(user.FullName))
        {
            user.FullName = user.Login;
        }

        if (user.AuthorizationType is null)
        {
            user.AuthorizationType = Domain.Enums.AuthorizationType.Password;
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
