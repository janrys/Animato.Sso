namespace Animato.Sso.Infrastructure.Services.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Animato.Sso.Application.Common.Interfaces;
using Animato.Sso.Domain.Entities;
using Microsoft.Extensions.Logging;

public class InMemoryUserRepository : IUserRepository
{
    private const string ERROR_LOADING_USERS = "Error loading users";
    private readonly InMemoryDataContext dataContext;
    private readonly ILogger<InMemoryUserRepository> logger;

    public InMemoryUserRepository(InMemoryDataContext dataContext, ILogger<InMemoryUserRepository> logger)
    {
        this.dataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
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

    public Task<User> GetUserByUserName(string userName, CancellationToken cancellationToken)
    {
        try
        {
            return Task.FromResult(dataContext.Users.FirstOrDefault(u => u.Login.Equals(userName, StringComparison.OrdinalIgnoreCase)));
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
}
