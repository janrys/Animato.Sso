namespace Animato.Sso.Infrastructure.Services.Persistence;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Animato.Sso.Application.Common.Interfaces;
using Animato.Sso.Application.Common.Logging;
using Animato.Sso.Domain.Entities;
using Microsoft.Extensions.Logging;

public class InMemoryAuthorizationCodeRepository : IAuthorizationCodeRepository
{
    private readonly List<AuthorizationCode> codes;
    private readonly ILogger<InMemoryAuthorizationCodeRepository> logger;

    public InMemoryAuthorizationCodeRepository(InMemoryDataContext dataContext, ILogger<InMemoryAuthorizationCodeRepository> logger)
    {
        if (dataContext is null)
        {
            throw new ArgumentNullException(nameof(dataContext));
        }

        codes = dataContext.Codes;
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task Delete(string code, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(code))
        {
            throw new ArgumentException($"'{nameof(code)}' cannot be null or empty.", nameof(code));
        }

        try
        {
            var storedCode = codes.FirstOrDefault(u => u.Code.Equals(code, StringComparison.Ordinal));

            if (storedCode is not null)
            {
                codes.Remove(storedCode);
            }

            return Task.CompletedTask;
        }
        catch (Exception exception)
        {
            logger.CodesDeletingError(exception);
            throw;
        }
    }

    public Task<int> DeleteExpired(DateTime expiration, CancellationToken cancellationToken)
    {
        try
        {
            var expiredCodes = codes.Where(u => u.Created <= expiration);

            if (expiredCodes.Any())
            {
                expiredCodes.ToList().ForEach(e => codes.Remove(e));
            }

            return Task.FromResult(expiredCodes.Count());
        }
        catch (Exception exception)
        {
            logger.CodesDeletingError(exception);
            throw;
        }
    }

    public Task<AuthorizationCode> GetCode(string code, CancellationToken cancellationToken)
    {
        try
        {
            return Task.FromResult(codes.FirstOrDefault(u => u.Code.Equals(code, StringComparison.Ordinal)));
        }
        catch (Exception exception)
        {
            logger.CodesLoadingError(exception);
            throw;
        }
    }

    public Task<AuthorizationCode> Create(AuthorizationCode code, CancellationToken cancellationToken)
    {
        try
        {
            codes.Add(code);
            return Task.FromResult(code);
        }
        catch (Exception exception)
        {
            logger.CodesInsertingError(exception);
            throw;
        }
    }
}
