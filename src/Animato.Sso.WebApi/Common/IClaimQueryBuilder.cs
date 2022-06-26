namespace Animato.Sso.WebApi.Common;
using Animato.Sso.Domain.Entities;

public interface IClaimQueryBuilder
{
    Task<IEnumerable<Claim>> GetAll();
    Task<Claim> GetByName(string name);
}
