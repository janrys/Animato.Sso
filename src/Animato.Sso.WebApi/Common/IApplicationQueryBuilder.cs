namespace Animato.Sso.WebApi.Common;
using Animato.Sso.Domain.Entities;

public interface IApplicationQueryBuilder
{
    Task<IEnumerable<Application>> GetAll();
    Task<Application> GetByClientId(string clientId);
    Task<Application> GetById(ApplicationId applicationId);
    Task<IEnumerable<ApplicationRole>> GetRolesByApplicationId(ApplicationId applicationId);
    Task<ApplicationRole> GetRoleById(ApplicationRoleId applicationRoleId);
    Task<IEnumerable<Scope>> GetScopes(ApplicationId applicationId);
}
