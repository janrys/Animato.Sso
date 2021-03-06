namespace Animato.Sso.WebApi.Common;
using Animato.Sso.Application.Features.Scopes.DTOs;
using Animato.Sso.Domain.Entities;

public interface IScopeCommandBuilder
{
    Task<IEnumerable<Scope>> Create(CreateScopesModel scopes);
    Task<Scope> Update(string oldName, string newName);
    Task Delete(string name);
    Task<IEnumerable<Claim>> AddClaim(string name, string claimName);
    Task<IEnumerable<Claim>> RemoveClaim(string name, string claimName);
}
