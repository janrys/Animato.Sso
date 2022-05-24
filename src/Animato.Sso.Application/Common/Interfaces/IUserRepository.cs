namespace Animato.Sso.Application.Common.Interfaces;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Animato.Sso.Domain.Entities;

public interface IUserRepository
{
    Task<IEnumerable<User>> GetUsers(CancellationToken cancellationToken);
    Task<User> GetUserByUserName(string userName, CancellationToken cancellationToken);
    Task<User> GetById(UserId userId, CancellationToken cancellationToken);
}
