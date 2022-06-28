namespace Animato.Sso.WebApi.Common;
using Animato.Sso.Domain.Entities;

public interface IScopeQueryBuilder
{
    Task<IEnumerable<Scope>> GetAll();
    Task<Scope> GetByName(string name);
    Task<IEnumerable<Claim>> GetClaims(string name);
}
