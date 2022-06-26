namespace Animato.Sso.WebApi.Common;

public interface IQueryBuilder
{
    IUserQueryBuilder User { get; }
    ITokenQueryBuilder Token { get; }
    IApplicationQueryBuilder Application { get; }
    IScopeQueryBuilder Scope { get; }
    IClaimQueryBuilder Claim { get; }
}
