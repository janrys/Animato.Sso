namespace Animato.Sso.Application.Common.Interfaces;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Animato.Sso.Domain.Entities;

public interface ITokenRepository
{
    Task<Token> GetToken(string token, CancellationToken cancellationToken);
    Task<Token> Insert(Token token, CancellationToken cancellationToken);
    Task<IEnumerable<Token>> Insert(Token accessToken, Token refreshToken, CancellationToken cancellationToken);
    Task<IEnumerable<Token>> Insert(Token accessToken, Token refreshToken, Token idToken, CancellationToken cancellationToken);
    Task<int> RemoveExpiredTokens();
}
