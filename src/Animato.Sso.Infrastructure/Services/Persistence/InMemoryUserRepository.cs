namespace Animato.Sso.Infrastructure.Services.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Animato.Sso.Application.Common.Interfaces;
using Animato.Sso.Application.Common.Logging;
using Animato.Sso.Application.Exceptions;
using Animato.Sso.Domain.Entities;
using Microsoft.Extensions.Logging;

public class InMemoryUserRepository : IUserRepository
{
    private readonly List<User> users;
    private readonly List<UserApplicationRole> userApplicationRoles;
    private readonly List<ApplicationRole> applicationRoles;
    private readonly List<UserClaim> userClaims;
    private readonly List<Claim> claims;
    private readonly List<Token> tokens;
    private readonly IDateTimeService dateTime;
    private readonly ILogger<InMemoryUserRepository> logger;

    public InMemoryUserRepository(InMemoryDataContext dataContext
        , IDateTimeService dateTime
        , ILogger<InMemoryUserRepository> logger)
    {
        if (dataContext is null)
        {
            throw new ArgumentNullException(nameof(dataContext));
        }

        users = dataContext.Users;
        userApplicationRoles = dataContext.UserApplicationRoles;
        applicationRoles = dataContext.ApplicationRoles;
        userClaims = dataContext.UserClaims;
        claims = dataContext.Claims;
        tokens = dataContext.Tokens;
        this.dateTime = dateTime ?? throw new ArgumentNullException(nameof(dateTime));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<User> Create(User user, CancellationToken cancellationToken)
        => Create(user, UserId.New(), cancellationToken);

    public Task<User> Create(User user, UserId userId, CancellationToken cancellationToken)
    {
        if (user is null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        try
        {
            user.Id = userId;
            user.LastChanged = dateTime.UtcNow;
            users.Add(user);
            return Task.FromResult(user);
        }
        catch (Exception exception)
        {
            logger.UsersCreatingError(exception);
            throw;
        }
    }

    public Task<User> Update(User user, CancellationToken cancellationToken)
    {
        if (user is null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        try
        {
            var storedUser = users.FirstOrDefault(a => a.Id == user.Id);

            if (storedUser == null)
            {
                throw new NotFoundException(nameof(User), user.Id);
            }

            user.LastChanged = dateTime.UtcNow;
            users.Remove(storedUser);
            users.Add(user);

            return Task.FromResult(user);
        }
        catch (Exception exception)
        {
            logger.UsersUpdatingError(exception);
            throw;
        }
    }

    public Task DeleteForce(UserId userId, CancellationToken cancellationToken)
    {
        try
        {
            userClaims.RemoveAll(a => a.UserId == userId);
            tokens.RemoveAll(a => a.UserId == userId);
            return Task.FromResult(users.RemoveAll(a => a.Id == userId));
        }
        catch (Exception exception)
        {
            logger.UsersDeletingError(exception);
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
            logger.UsersDeletingError(exception);
            throw;
        }
    }

    public Task<User> GetById(UserId userId, CancellationToken cancellationToken)
    {
        try
        {
            return Task.FromResult(users.FirstOrDefault(u => u.Id == userId));
        }
        catch (Exception exception)
        {
            logger.UsersLoadingError(exception);
            throw;
        }
    }

    public Task<User> GetUserByLogin(string login, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(login))
        {
            throw new ArgumentException($"'{nameof(login)}' cannot be null or empty.", nameof(login));
        }

        try
        {
            return Task.FromResult(users.FirstOrDefault(u => u.Login.Equals(login, StringComparison.OrdinalIgnoreCase)));
        }
        catch (Exception exception)
        {
            logger.UsersLoadingError(exception);
            throw;
        }
    }

    public Task<IEnumerable<User>> GetUsers(CancellationToken cancellationToken)
    {
        try
        {
            return Task.FromResult(users.AsEnumerable());
        }
        catch (Exception exception)
        {
            logger.UsersLoadingError(exception);
            throw;
        }
    }

    public Task<IEnumerable<User>> GetUserByRole(ApplicationRoleId roleId, CancellationToken cancellationToken)
    {
        try
        {
            return Task.FromResult(userApplicationRoles.Where(r => r.ApplicationRoleId == roleId)
                .Join(users, r => r.UserId, u => u.Id, (r, u) => u));
        }
        catch (Exception exception)
        {
            logger.UsersLoadingError(exception);
            throw;
        }
    }

    public Task<IEnumerable<ApplicationRole>> GetUserRoles(UserId userId, CancellationToken cancellationToken)
    {
        try
        {
            return Task.FromResult(userApplicationRoles.Where(r => r.UserId == userId)
                .Join(applicationRoles, r => r.ApplicationRoleId, ar => ar.Id, (r, ar) => ar));
        }
        catch (Exception exception)
        {
            logger.ApplicationRolesLoadingError(exception);
            throw;
        }
    }

    public async Task AddUserRole(UserId userId, ApplicationRoleId roleId, CancellationToken cancellationToken)
    {
        try
        {
            userApplicationRoles.Add(new UserApplicationRole()
            {
                UserId = userId,
                ApplicationRoleId = roleId,
            });
            await UpdateUserLastChange(userId, cancellationToken);
        }
        catch (Exception exception)
        {
            logger.ApplicationRolesCreatingError(exception);
            throw;
        }
    }

    public async Task AddUserRoles(UserId userId, CancellationToken cancellationToken, params ApplicationRoleId[] roleIds)
    {
        if (roleIds is null || !roleIds.Any())
        {
            return;
        }

        try
        {
            roleIds.ToList().ForEach(r => userApplicationRoles.Add(new UserApplicationRole()
            {
                UserId = userId,
                ApplicationRoleId = r,
            }));
            await UpdateUserLastChange(userId, cancellationToken);
        }
        catch (Exception exception)
        {
            logger.ApplicationRolesCreatingError(exception);
            throw;
        }
    }

    public async Task RemoveUserRole(UserId userId, ApplicationRoleId roleId, CancellationToken cancellationToken)
    {
        try
        {
            userApplicationRoles.RemoveAll(r => r.UserId == userId && r.ApplicationRoleId == roleId);
            await UpdateUserLastChange(userId, cancellationToken);
        }
        catch (Exception exception)
        {
            logger.ApplicationRolesDeletingError(exception);
            throw;
        }
    }

    private Task UpdateUserLastChange(UserId userId, CancellationToken cancellationToken)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var user = users.FirstOrDefault(u => u.Id == userId);

            if (user is not null)
            {
                user.LastChanged = dateTime.UtcNow;
            }

            return Task.CompletedTask;
        }
        catch (Exception exception)
        {
            logger.UsersUpdatingError(exception);
            throw;
        }
    }

    public Task ClearRoles(CancellationToken cancellationToken)
    {
        userApplicationRoles.Clear();
        return Task.CompletedTask;
    }

    public Task Clear(CancellationToken cancellationToken)
    {
        users.Clear();
        return Task.CompletedTask;
    }

    public Task<IEnumerable<UserClaim>> GetClaims(ClaimId id, int topCount, CancellationToken cancellationToken)
    {
        if (topCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(topCount), topCount, "Must be greater than 0");
        }

        try
        {
            var storedUserClaims = userClaims.Where(c => c.ClaimId == id);

            if (!storedUserClaims.Any())
            {
                return Task.FromResult(Enumerable.Empty<UserClaim>());
            }

            return Task.FromResult(storedUserClaims.Take(topCount));
        }
        catch (Exception exception)
        {
            logger.ClaimsLoadingError(exception);
            throw;
        }
    }
}
