namespace Animato.Sso.Application.Common.Interfaces;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Animato.Sso.Domain.Entities;

public interface IApplicationRoleRepository
{
    Task<ApplicationRole> GetById(ApplicationRoleId applicationRoleId, CancellationToken cancellationToken);
    Task<IEnumerable<ApplicationRole>> GetByIds(CancellationToken cancellationToken, params ApplicationRoleId[] applicationRoleIds);
    Task<IEnumerable<ApplicationRole>> GetByApplicationId(ApplicationId applicationId, CancellationToken cancellationToken);
    Task Delete(ApplicationRoleId roleId, CancellationToken cancellationToken);
    Task<ApplicationRole> Create(ApplicationRole role, CancellationToken cancellationToken);
    Task<IEnumerable<ApplicationRole>> Create(CancellationToken cancellationToken, params ApplicationRole[] roles);
    Task<ApplicationRole> Update(ApplicationRole role, CancellationToken cancellationToken);
    Task Clear(CancellationToken cancellationToken);
}
