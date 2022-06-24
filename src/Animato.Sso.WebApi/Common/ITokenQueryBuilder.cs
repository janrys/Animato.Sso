namespace Animato.Sso.WebApi.Common;
using Animato.Sso.Application.Models;

public interface ITokenQueryBuilder
{
    Task<TokenInfo> GetTokenInfo(string token);
}
