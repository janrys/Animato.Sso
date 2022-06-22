namespace Animato.Sso.Application.Common.Interfaces;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Animato.Sso.Domain.Entities;

public interface IUserRepository
{
    Task<IEnumerable<User>> GetUsers(CancellationToken cancellationToken);
    Task<User> GetUserByLogin(string login, CancellationToken cancellationToken);
    Task<User> GetById(UserId userId, CancellationToken cancellationToken);

    /// <summary>
    /// Set user flag as deleted
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task DeleteSoft(UserId userId, CancellationToken cancellationToken);

    /// <summary>
    /// Remove user from database
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task DeleteForce(UserId userId, CancellationToken cancellationToken);
    Task<User> Create(User user, CancellationToken cancellationToken);
    Task<User> Create(User user, UserId userId, CancellationToken cancellationToken);
    Task<User> Update(User user, CancellationToken cancellationToken);
    Task<IEnumerable<User>> GetUserByRole(ApplicationRoleId roleId, CancellationToken cancellationToken);
    Task<IEnumerable<ApplicationRole>> GetUserRoles(UserId userId, CancellationToken cancellationToken);
    Task AddUserRole(UserId userId, ApplicationRoleId roleId, CancellationToken cancellationToken);
    Task AddUserRoles(UserId userId, CancellationToken cancellationToken, params ApplicationRoleId[] roleIds);
    Task RemoveUserRole(UserId userId, ApplicationRoleId roleId, CancellationToken cancellationToken);
    Task ClearRoles(CancellationToken cancellationToken);
    Task Clear(CancellationToken cancellationToken);
}
