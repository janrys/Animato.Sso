namespace Animato.Sso.Application.Common.Interfaces;

using Animato.Sso.Domain.Entities;

public interface ITokenFactory
{
    string GenerateAccessToken(User user, Application application, params ApplicationRole[] roles);
    string GenerateCode();
    string GenerateRefreshToken(User user);
    string GenerateIdToken(User user, Application application, params ApplicationRole[] roles);
}
