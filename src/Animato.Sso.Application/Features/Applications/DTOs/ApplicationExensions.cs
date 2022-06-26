namespace Animato.Sso.Application.Features.Applications.DTOs;

using System;
using System.Collections.Generic;
using System.Linq;
using Animato.Sso.Application.Common;
using Animato.Sso.Application.Common.Interfaces;

public static class ApplicationExensions
{

    public static Domain.Entities.Application ApplyTo(this CreateApplicationModel model, Domain.Entities.Application application)
    {
        application.Name = model.Name;
        application.Code = model.Code;
        application.AuthorizationMethod = model.AuthorizationMethod;
        application.AccessTokenExpirationMinutes = model.AccessTokenExpirationMinutes.Value;
        application.RefreshTokenExpirationMinutes = model.RefreshTokenExpirationMinutes.Value;
        application.RedirectUris = model.RedirectUris;
        application.Secrets = model.Secrets;
        return application;
    }

    public static CreateApplicationModel ValidateAndSanitize(this CreateApplicationModel application
        , OidcOptions oidcOptions
        , ITokenFactory tokenFactory)
    {
        if (application is null)
        {
            throw new ArgumentNullException(nameof(application));
        }

        if (oidcOptions is null)
        {
            throw new ArgumentNullException(nameof(oidcOptions));
        }

        if (tokenFactory is null)
        {
            throw new ArgumentNullException(nameof(tokenFactory));
        }

        if (application.RedirectUris is null || !application.RedirectUris.Any())
        {
            throw new Exceptions.ValidationException(
                Exceptions.ValidationException.CreateFailure(nameof(application.RedirectUris)
                , $"Application must have at least one redirect URI"));
        }

        if (application.Secrets is null || !application.Secrets.Any())
        {
            application.Secrets = new List<string>
                {
                    tokenFactory.GenerateRandomString(oidcOptions.ApplicationSecretLength)
                };
        }

        if (!application.AccessTokenExpirationMinutes.HasValue || application.AccessTokenExpirationMinutes.Value <= 0)
        {
            application.AccessTokenExpirationMinutes = oidcOptions.AccessTokenExpirationMinutes;
        }

        if (!application.RefreshTokenExpirationMinutes.HasValue || application.AccessTokenExpirationMinutes.Value <= 0)
        {
            application.RefreshTokenExpirationMinutes = oidcOptions.RefreshTokenExpirationMinutes;
        }

        if (application.AuthorizationMethod is null)
        {
            application.AuthorizationMethod = Domain.Enums.AuthorizationMethod.Password;
        }

        return application;
    }
}
