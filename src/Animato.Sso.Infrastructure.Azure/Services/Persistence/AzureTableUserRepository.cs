namespace Animato.Sso.Infrastructure.Azure.Services.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Animato.Sso.Application.Common.Interfaces;
using Animato.Sso.Application.Exceptions;
using Animato.Sso.Domain.Entities;
using Microsoft.Extensions.Logging;

public class AzureTableUserRepository : IUserRepository
{
    private const string ERROR_LOADING_USERS = "Error loading users";
    private const string ERROR_INSERTING_USERS = "Error inserting users";
    private const string ERROR_UPDATING_USERS = "Error updating users";
    private const string ERROR_DELETING_USERS = "Error deleting users";
    private readonly AzureTableStorageDataContext dataContext;
    private readonly ILogger<AzureTableUserRepository> logger;

    public AzureTableUserRepository(AzureTableStorageDataContext dataContext, ILogger<AzureTableUserRepository> logger)
    {
        this.dataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<User> Create(User user, CancellationToken cancellationToken)
    {
        await dataContext.ThrowExceptionIfTableNotExists(cancellationToken);

        try
        {

            user.Id = UserId.New();
            user.LastChanged = DateTime.UtcNow;
            dataContext.Users.Add(user);
            return Task.FromResult(user);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, ERROR_INSERTING_USERS);
            throw;
        }
    }

    public async Task<User> Update(User user, CancellationToken cancellationToken)
    {
        await dataContext.ThrowExceptionIfTableNotExists(cancellationToken);

        try
        {
            var storedUser = dataContext.Users.FirstOrDefault(a => a.Id == user.Id);

            if (storedUser == null)
            {
                throw new NotFoundException(nameof(User), user.Id);
            }

            user.LastChanged = DateTime.UtcNow;
            dataContext.Users.Remove(storedUser);
            dataContext.Users.Add(user);

            return Task.FromResult(user);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, ERROR_UPDATING_USERS);
            throw;
        }
    }

    public async Task DeleteForce(UserId userId, CancellationToken cancellationToken)
    {
        await dataContext.ThrowExceptionIfTableNotExists(cancellationToken);

        try
        {
            return Task.FromResult(dataContext.Users.RemoveAll(a => a.Id == userId));
        }
        catch (Exception exception)
        {
            logger.LogError(exception, ERROR_DELETING_USERS);
            throw;
        }
    }

    public async Task DeleteSoft(UserId userId, CancellationToken cancellationToken)
    {
        await dataContext.ThrowExceptionIfTableNotExists(cancellationToken);

        try
        {
            var user = await GetById(userId, cancellationToken);

            if (user == null)
            {
                throw new NotFoundException(nameof(User), userId);
            }

            if (user.IsDeleted)
            {
                return;
            }

            user.IsDeleted = true;
            await Update(user, cancellationToken);

        }
        catch (Exception exception)
        {
            logger.LogError(exception, ERROR_UPDATING_USERS);
            throw;
        }
    }

    public Task<User> GetById(UserId userId, CancellationToken cancellationToken)
    {
        await dataContext.ThrowExceptionIfTableNotExists(cancellationToken);

        try
        {
            return Task.FromResult(dataContext.Users.FirstOrDefault(u => u.Id == userId));
        }
        catch (Exception exception)
        {
            logger.LogError(exception, ERROR_LOADING_USERS);
            throw;
        }
    }

    public Task<User> GetUserByLogin(string login, CancellationToken cancellationToken)
    {
        await dataContext.ThrowExceptionIfTableNotExists(cancellationToken);

        try
        {
            return Task.FromResult(dataContext.Users.FirstOrDefault(u => u.Login.Equals(login, StringComparison.OrdinalIgnoreCase)));
        }
        catch (Exception exception)
        {
            logger.LogError(exception, ERROR_LOADING_USERS);
            throw;
        }
    }

    public async Task<IEnumerable<User>> GetUsers(CancellationToken cancellationToken)
    {
        await dataContext.ThrowExceptionIfTableNotExists(cancellationToken);

        try
        {
            return Task.FromResult(dataContext.Users.AsEnumerable());
        }
        catch (Exception exception)
        {
            logger.LogError(exception, ERROR_LOADING_USERS);
            throw;
        }
    }

    public async Task<IEnumerable<User>> GetUserByRole(ApplicationRoleId roleId)
    {
        await dataContext.ThrowExceptionIfTableNotExists(cancellationToken);

        try
        {
            return Task.FromResult(dataContext.UserApplicationRoles.Where(r => r.ApplicationRoleId == roleId)
                .Join(dataContext.Users, r => r.UserId, u => u.Id, (r, u) => u));
        }
        catch (Exception exception)
        {
            logger.LogError(exception, ERROR_LOADING_USERS);
            throw;
        }
    }

    public async Task<IEnumerable<ApplicationRole>> GetUserRoles(UserId userId, CancellationToken cancellationToken)
    {
        await dataContext.ThrowExceptionIfTableNotExists(cancellationToken);

        try
        {
            return Task.FromResult(dataContext.UserApplicationRoles.Where(r => r.UserId == userId)
                .Join(dataContext.ApplicationRoles, r => r.ApplicationRoleId, ar => ar.Id, (r, ar) => ar));
        }
        catch (Exception exception)
        {
            logger.LogError(exception, ERROR_LOADING_USERS);
            throw;
        }
    }

    public async Task AddUserRole(UserId userId, ApplicationRoleId roleId, CancellationToken cancellationToken)
    {
        await dataContext.ThrowExceptionIfTableNotExists(cancellationToken);

        try
        {
            dataContext.UserApplicationRoles.Add(new UserApplicationRole()
            {
                UserId = userId,
                ApplicationRoleId = roleId,
            });
            await UpdateUserLastChange(userId, cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, ERROR_UPDATING_USERS);
            throw;
        }
    }

    public async Task RemoveUserRole(UserId userId, ApplicationRoleId roleId, CancellationToken cancellationToken)
    {
        await dataContext.ThrowExceptionIfTableNotExists(cancellationToken);

        try
        {
            dataContext.UserApplicationRoles.RemoveAll(r => r.UserId == userId && r.ApplicationRoleId == roleId);
            await UpdateUserLastChange(userId, cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, ERROR_UPDATING_USERS);
            throw;
        }
    }

    private async Task UpdateUserLastChange(UserId userId, CancellationToken cancellationToken)
    {
        await dataContext.ThrowExceptionIfTableNotExists(cancellationToken);

        try
        {
            var user = dataContext.Users.FirstOrDefault(u => u.Id == userId);

            if (user is not null)
            {
                user.LastChanged = DateTime.UtcNow;
            }

            return Task.CompletedTask;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, ERROR_UPDATING_USERS);
            throw;
        }
    }
}
