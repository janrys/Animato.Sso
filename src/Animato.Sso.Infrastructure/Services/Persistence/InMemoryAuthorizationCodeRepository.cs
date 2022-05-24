namespace Animato.Sso.Infrastructure.Services.Persistence;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Animato.Sso.Application.Common.Interfaces;
using Animato.Sso.Domain.Entities;
using Microsoft.Extensions.Logging;

public class InMemoryAuthorizationCodeRepository : IAuthorizationCodeRepository
{
    private const string ERROR_LOADING_CODES = "Error loading codes";
    private const string ERROR_INSERTING_CODES = "Error inserting codes";
    private const string ERROR_DELETING_CODES = "Error deleting codes";
    private readonly InMemoryDataContext dataContext;
    private readonly ILogger<InMemoryAuthorizationCodeRepository> logger;

    public InMemoryAuthorizationCodeRepository(InMemoryDataContext dataContext, ILogger<InMemoryAuthorizationCodeRepository> logger)
    {
        this.dataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task Delete(string code, CancellationToken cancellationToken)
    {
        try
        {
            var storedCode = dataContext.Codes.FirstOrDefault(u => u.Code.Equals(code, StringComparison.Ordinal));

            if (storedCode is not null)
            {
                dataContext.Codes.Remove(storedCode);
            }

            return Task.CompletedTask;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, ERROR_DELETING_CODES);
            throw;
        }
    }

    public Task<int> DeleteExpired(DateTime expiration, CancellationToken cancellationToken)
    {
        try
        {
            var expiredCodes = dataContext.Codes.Where(u => u.Created <= expiration);

            if (expiredCodes.Any())
            {
                expiredCodes.ToList().ForEach(e => dataContext.Codes.Remove(e));
            }

            return Task.FromResult(expiredCodes.Count());
        }
        catch (Exception exception)
        {
            logger.LogError(exception, ERROR_DELETING_CODES);
            throw;
        }
    }

    public Task<AuthorizationCode> GetCode(string code, CancellationToken cancellationToken)
    {
        try
        {
            return Task.FromResult(dataContext.Codes.FirstOrDefault(u => u.Code.Equals(code, StringComparison.Ordinal)));
        }
        catch (Exception exception)
        {
            logger.LogError(exception, ERROR_LOADING_CODES);
            throw;
        }
    }

    public Task<AuthorizationCode> Insert(AuthorizationCode code, CancellationToken cancellationToken)
    {
        try
        {
            dataContext.Codes.Add(code);
            return Task.FromResult(code);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, ERROR_INSERTING_CODES);
            throw;
        }
    }
}
