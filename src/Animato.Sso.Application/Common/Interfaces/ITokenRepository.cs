namespace Animato.Sso.Application.Common.Interfaces;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Animato.Sso.Domain.Entities;

public interface ITokenRepository
{
    Task<Token> GetToken(string token, CancellationToken cancellationToken);
    Task<Token> Create(Token token, CancellationToken cancellationToken);
    Task<IEnumerable<Token>> Create(Token accessToken, Token refreshToken, CancellationToken cancellationToken);
    Task<IEnumerable<Token>> Create(Token accessToken, Token refreshToken, Token idToken, CancellationToken cancellationToken);
    Task<int> DeleteExpiredTokens(CancellationToken cancellationToken);
    Task Revoke(string token, CancellationToken cancellationToken);
    Task RevokeTokensForUser(UserId id, CancellationToken cancellationToken);
}
