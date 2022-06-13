namespace Animato.Sso.Infrastructure.Services.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Animato.Sso.Application.Common.Interfaces;
using Animato.Sso.Application.Exceptions;
using Animato.Sso.Domain.Entities;
using Microsoft.Extensions.Logging;

public class InMemoryUserRepository : IUserRepository
{
    private const string ERROR_LOADING_USERS = "Error loading users";
    private const string ERROR_INSERTING_USERS = "Error inserting users";
    private const string ERROR_UPDATING_USERS = "Error updating users";
    private const string ERROR_DELETING_USERS = "Error deleting users";
    private readonly InMemoryDataContext dataContext;
    private readonly ILogger<InMemoryUserRepository> logger;

    public InMemoryUserRepository(InMemoryDataContext dataContext, ILogger<InMemoryUserRepository> logger)
    {
        this.dataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<User> Create(User user, CancellationToken cancellationToken)
    {
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

    public Task<User> Update(User user, CancellationToken cancellationToken)
    {
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

    public Task DeleteForce(UserId userId, CancellationToken cancellationToken)
    {
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

    public Task<IEnumerable<User>> GetUsers(CancellationToken cancellationToken)
    {
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

    public Task<IEnumerable<User>> GetUserByRole(ApplicationRoleId roleId)
    {
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
}
