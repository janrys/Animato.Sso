namespace Animato.Sso.WebApi.Common;
using Animato.Sso.Application.Features.Applications.DTOs;
using Animato.Sso.Domain.Entities;

public interface IApplicationCommandBuilder
{
    Task<Application> Create(CreateApplicationModel application);
    Task<Application> Update(ApplicationId applicationId, CreateApplicationModel application);
    Task Delete(ApplicationId applicationId);
    Task<IEnumerable<ApplicationRole>> CreateRole(ApplicationId applicationId, CreateApplicationRolesModel roles);
    Task<ApplicationRole> UpdateRole(ApplicationRoleId roleId, CreateApplicationRoleModel role);
    Task DeleteRole(ApplicationRoleId roleId);
}
