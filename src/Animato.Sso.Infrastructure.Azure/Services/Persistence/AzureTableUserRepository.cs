namespace Animato.Sso.Infrastructure.AzureStorage.Services.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Animato.Sso.Application.Common.Interfaces;
using Animato.Sso.Application.Common.Logging;
using Animato.Sso.Application.Exceptions;
using Animato.Sso.Domain.Entities;
using Animato.Sso.Infrastructure.AzureStorage.Services.Persistence.DTOs;
using Azure.Data.Tables;
using Microsoft.Extensions.Logging;

public class AzureTableUserRepository : IUserRepository
{
    private TableClient TableUsers => dataContext.Users;
    private TableClient TableUserApplicationRoles => dataContext.UserApplicationRoles;
    private TableClient TableApplicationRoles => dataContext.ApplicationRoles;
    private TableClient TableUserClaims => dataContext.UserClaims;
    private Func<CancellationToken, Task> CheckIfTableExists => dataContext.ThrowExceptionIfTableNotExists;
    private readonly AzureTableStorageDataContext dataContext;
    private readonly IDateTimeService dateTime;
    private readonly ILogger<AzureTableUserRepository> logger;

    public AzureTableUserRepository(AzureTableStorageDataContext dataContext
        , IDateTimeService dateTime
        , ILogger<AzureTableUserRepository> logger)
    {
        this.dataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
        this.dateTime = dateTime ?? throw new ArgumentNullException(nameof(dateTime));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    private Task ThrowExceptionIfTableNotExists(CancellationToken cancellationToken)
        => CheckIfTableExists(cancellationToken);


    public Task<User> Create(User user, CancellationToken cancellationToken)
        => Create(user, UserId.New(), cancellationToken);

    public async Task<User> Create(User user, UserId id, CancellationToken cancellationToken)
    {
        if (user is null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        await ThrowExceptionIfTableNotExists(cancellationToken);

        try
        {
            user.Id = id;
            user.LastChanged = dateTime.UtcNow;
            var tableEntity = user.ToTableEntity();
            await TableUsers.AddEntityAsync(tableEntity, cancellationToken);
            return user;
        }
        catch (Exception exception)
        {
            logger.UsersCreatingError(exception);
            throw;
        }
    }

    public async Task<User> Update(User user, CancellationToken cancellationToken)
    {
        await ThrowExceptionIfTableNotExists(cancellationToken);

        try
        {
            user.LastChanged = dateTime.UtcNow;
            var tableEntity = user.ToTableEntity();
            await TableUsers.UpdateEntityAsync(tableEntity, Azure.ETag.All, cancellationToken: cancellationToken);
            return user;
        }
        catch (Exception exception)
        {
            logger.UsersUpdatingError(exception);
            throw;
        }
    }

    public async Task DeleteForce(UserId id, CancellationToken cancellationToken)
    {
        await ThrowExceptionIfTableNotExists(cancellationToken);

        var user = await GetById(id, cancellationToken);

        if (user == null)
        {
            return;
        }

        var tableEntity = user.ToTableEntity();

        try
        {
            await TableUsers.DeleteEntityAsync(tableEntity.PartitionKey, tableEntity.RowKey, cancellationToken: cancellationToken);

        }
        catch (Exception exception)
        {
            logger.UsersDeletingError(exception);
            throw;
        }
    }

    public async Task DeleteSoft(UserId id, CancellationToken cancellationToken)
    {
        var user = await GetById(id, cancellationToken);

        if (user == null)
        {
            throw new NotFoundException(nameof(User), id);
        }

        if (user.IsDeleted)
        {
            return;
        }

        user.IsDeleted = true;
        await Update(user, cancellationToken);
    }

    public async Task<User> GetById(UserId id, CancellationToken cancellationToken)
    {
        await ThrowExceptionIfTableNotExists(cancellationToken);

        try
        {
            var results = new List<UserTableEntity>();
            var queryResult = TableUsers.QueryAsync<UserTableEntity>(a => a.RowKey == id.Value.ToString(), cancellationToken: cancellationToken);

            await queryResult.AsPages()
                .ForEachAsync(page => results.AddRange(page.Values), cancellationToken)
                .ConfigureAwait(false);

            if (results.Count == 1)
            {
                return results.First().ToEntity();
            }

            if (results.Count == 0)
            {
                return null;
            }

            throw new DataAccessException($"Found duplicate users ({results.Count}) for id {id.Value}");
        }
        catch (Exception exception)
        {
            logger.UsersLoadingError(exception);
            throw;
        }
    }

    public async Task<User> GetUserByLogin(string login, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(login))
        {
            throw new ArgumentException($"'{nameof(login)}' cannot be null or empty.", nameof(login));
        }

        await ThrowExceptionIfTableNotExists(cancellationToken);

        try
        {
            login = login.ToLowerInvariant();
            var results = new List<UserTableEntity>();
            var queryResult = TableUsers.QueryAsync<UserTableEntity>(a => a.PartitionKey == login, cancellationToken: cancellationToken);

            await queryResult.AsPages()
                .ForEachAsync(page => results.AddRange(page.Values), cancellationToken)
                .ConfigureAwait(false);

            if (results.Count == 1)
            {
                return results.First().ToEntity();
            }

            if (results.Count == 0)
            {
                return null;
            }

            throw new DataAccessException($"Found duplicate users ({results.Count}) for id {login}");
        }
        catch (Exception exception)
        {
            logger.UsersLoadingError(exception);
            throw;
        }
    }

    public async Task<IEnumerable<User>> GetAll(CancellationToken cancellationToken)
    {
        await ThrowExceptionIfTableNotExists(cancellationToken);

        try
        {
            var results = new List<UserTableEntity>();
            var queryResult = TableUsers.QueryAsync<UserTableEntity>(cancellationToken: cancellationToken);

            await queryResult.AsPages()
                .ForEachAsync(page => results.AddRange(page.Values), cancellationToken)
                .ConfigureAwait(false);

            return results.Select(u => u.ToEntity());
        }
        catch (Exception exception)
        {
            logger.UsersLoadingError(exception);
            throw;
        }
    }

    public async Task<IEnumerable<User>> GetUserByRole(ApplicationRoleId roleId, CancellationToken cancellationToken)
    {
        await ThrowExceptionIfTableNotExists(cancellationToken);

        try
        {
            var userApplicationRoles = new List<UserApplicationRoleTableEntity>();
            var queryResult = TableUserApplicationRoles
                .QueryAsync<UserApplicationRoleTableEntity>(r => r.PartitionKey == roleId.Value.ToString()
                , cancellationToken: cancellationToken);

            await queryResult.AsPages()
                .ForEachAsync(page => userApplicationRoles.AddRange(page.Values), cancellationToken)
                .ConfigureAwait(false);

            if (userApplicationRoles.Count == 0)
            {
                return Enumerable.Empty<User>();
            }

            var users = new List<User>();

            foreach (var userRole in userApplicationRoles)
            {
                var user = await GetById(userRole.ToEntity().UserId, cancellationToken);

                if (user is not null)
                {
                    users.Add(user);
                }
            }

            return users;
        }
        catch (Exception exception)
        {
            logger.UsersLoadingError(exception);
            throw;
        }
    }

    public async Task<IEnumerable<ApplicationRole>> GetUserRoles(UserId id, CancellationToken cancellationToken)
    {
        await ThrowExceptionIfTableNotExists(cancellationToken);

        try
        {
            var userApplicationRoles = new List<UserApplicationRoleTableEntity>();
            var queryResult = TableUserApplicationRoles
                .QueryAsync<UserApplicationRoleTableEntity>(r => r.RowKey == id.Value.ToString()
                , cancellationToken: cancellationToken);

            await queryResult.AsPages()
                .ForEachAsync(page => userApplicationRoles.AddRange(page.Values), cancellationToken)
                .ConfigureAwait(false);

            if (userApplicationRoles.Count == 0)
            {
                return Enumerable.Empty<ApplicationRole>();
            }

            var applicationRoles = new List<ApplicationRoleTableEntity>();

            foreach (var userRole in userApplicationRoles)
            {
                var queryResultApplicationRoles = TableApplicationRoles
                .QueryAsync<ApplicationRoleTableEntity>(r => r.RowKey == userRole.ApplicationRoleId
                , cancellationToken: cancellationToken);

                await queryResultApplicationRoles.AsPages()
                    .ForEachAsync(page => applicationRoles.AddRange(page.Values), cancellationToken)
                    .ConfigureAwait(false);
            }

            return applicationRoles.Select(r => r.ToEntity());
        }
        catch (Exception exception)
        {
            logger.ApplicationRolesLoadingError(exception);
            throw;
        }
    }

    public async Task AddUserRole(UserId userId, ApplicationRoleId roleId, CancellationToken cancellationToken)
    {
        await ThrowExceptionIfTableNotExists(cancellationToken);

        try
        {
            var tableEntity = new UserApplicationRoleTableEntity(roleId, userId);
            await TableUserApplicationRoles.AddEntityAsync(tableEntity, cancellationToken);
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

        await ThrowExceptionIfTableNotExists(cancellationToken);

        try
        {
            await AzureTableStorageDataContext.BatchManipulateEntities(
                TableUserApplicationRoles
                , roleIds.Select(r => new UserApplicationRole()
                {
                    UserId = userId,
                    ApplicationRoleId = r,
                }.ToTableEntity())
                , TableTransactionActionType.Add
                , cancellationToken
                );
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
        await ThrowExceptionIfTableNotExists(cancellationToken);

        try
        {
            var tableEntity = new UserApplicationRoleTableEntity(roleId, userId);
            await TableUserApplicationRoles.DeleteEntityAsync(tableEntity.PartitionKey, tableEntity.RowKey, cancellationToken: cancellationToken);
            await UpdateUserLastChange(userId, cancellationToken);
        }
        catch (Exception exception)
        {
            logger.ApplicationRolesDeletingError(exception);
            throw;
        }
    }

    private async Task UpdateUserLastChange(UserId userId, CancellationToken cancellationToken)
    {
        var user = await GetById(userId, cancellationToken);

        if (user is not null)
        {
            user.LastChanged = dateTime.UtcNow;
        }

        await Update(user, cancellationToken);
    }

    public async Task ClearRoles(CancellationToken cancellationToken)
    {
        await ThrowExceptionIfTableNotExists(cancellationToken);

        try
        {
            await AzureTableStorageDataContext.DeleteAllEntitiesAsync(TableUserApplicationRoles, CancellationToken.None);
        }
        catch (Exception exception)
        {
            logger.ApplicationRolesDeletingError(exception);
            throw;
        }
    }

    public async Task Clear(CancellationToken cancellationToken)
    {
        await ThrowExceptionIfTableNotExists(cancellationToken);

        try
        {
            await AzureTableStorageDataContext.DeleteAllEntitiesAsync(TableUsers, CancellationToken.None);
        }
        catch (Exception exception)
        {
            logger.ApplicationRolesDeletingError(exception);
            throw;
        }
    }

    public async Task<IEnumerable<UserClaim>> GetClaims(ClaimId claimId, int topCount, CancellationToken cancellationToken)
    {
        if (topCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(topCount), topCount, "Must be greater than 0");
        }

        try
        {
            var results = new List<UserClaimTableEntity>();
            var queryResult = TableUsers.QueryAsync<UserClaimTableEntity>(a => a.RowKey == claimId.Value.ToString(), cancellationToken: cancellationToken);

            await foreach (var page in queryResult.AsPages())
            {
                results.AddRange(page.Values);

                if (results.Count >= topCount)
                {
                    break;
                }
            }

            return results.Take(topCount).Select(c => c.ToEntity());
        }
        catch (Exception exception)
        {
            logger.ClaimsLoadingError(exception);
            throw;
        }
    }
}
