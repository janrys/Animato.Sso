namespace Animato.Sso.WebApi.Common;

public interface ITokenCommandBuilder
{
    Task RevokeToken(string token);
    Task RevokeAllTokens();
}
