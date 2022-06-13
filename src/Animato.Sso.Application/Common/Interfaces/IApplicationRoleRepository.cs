namespace Animato.Sso.Application.Common.Interfaces;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Animato.Sso.Domain.Entities;

public interface IApplicationRoleRepository
{
    Task<ApplicationRole> GetById(ApplicationRoleId applicationRoleId, CancellationToken cancellationToken);
    Task<IEnumerable<ApplicationRole>> GetByApplicationId(ApplicationId applicationId, CancellationToken cancellationToken);
    Task Delete(ApplicationRoleId roleId, CancellationToken cancellationToken);
    Task<ApplicationRole> Create(ApplicationRole role, CancellationToken cancellationToken);
    Task<ApplicationRole> Update(ApplicationRole role, CancellationToken cancellationToken);
}
