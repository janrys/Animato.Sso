namespace Animato.Sso.Application.Common.Interfaces;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Animato.Sso.Domain.Entities;

public interface IUserRepository
{
    Task<IEnumerable<User>> GetAll(CancellationToken cancellationToken);
    Task<User> GetUserByLogin(string login, CancellationToken cancellationToken);
    Task<User> GetById(UserId id, CancellationToken cancellationToken);

    /// <summary>
    /// Set user flag as deleted
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task DeleteSoft(UserId id, CancellationToken cancellationToken);

    /// <summary>
    /// Remove user from database
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task DeleteForce(UserId id, CancellationToken cancellationToken);
    Task<User> Create(User user, CancellationToken cancellationToken);
    Task<User> Create(User user, UserId id, CancellationToken cancellationToken);
    Task<User> Update(User user, CancellationToken cancellationToken);
    Task<IEnumerable<User>> GetUserByRole(ApplicationRoleId roleId, CancellationToken cancellationToken);
    Task<IEnumerable<ApplicationRole>> GetUserRoles(UserId id, CancellationToken cancellationToken);
    Task AddUserRole(UserId userId, ApplicationRoleId roleId, CancellationToken cancellationToken);
    Task AddUserRoles(UserId userId, CancellationToken cancellationToken, params ApplicationRoleId[] roleIds);
    Task RemoveUserRole(UserId userId, ApplicationRoleId roleId, CancellationToken cancellationToken);
    Task ClearRoles(CancellationToken cancellationToken);
    Task Clear(CancellationToken cancellationToken);
    Task<IEnumerable<UserClaim>> GetClaims(ClaimId claimId, int topCount, CancellationToken cancellationToken);
}
