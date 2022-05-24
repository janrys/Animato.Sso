namespace Animato.Sso.Application.Common.Interfaces;

using Animato.Sso.Domain.Entities;

public interface IAuthorizationCodeRepository
{
    Task<AuthorizationCode> GetCode(string code, CancellationToken cancellationToken);
    Task Delete(string code, CancellationToken cancellationToken);
    Task<AuthorizationCode> Insert(AuthorizationCode code, CancellationToken cancellationToken);
    Task<int> DeleteExpired(DateTime expiration, CancellationToken cancellationToken);
}
