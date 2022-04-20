namespace Animato.Sso.Application.Common.Behaviours;
using System.Diagnostics;
using Animato.Sso.Application.Common.Interfaces;
using Animato.Sso.Application.Security;
using MediatR;
using Microsoft.Extensions.Logging;

public class PerformanceBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    private readonly Stopwatch timer;
    private readonly ILogger<TRequest> logger;
    private readonly ICurrentUserService currentUserService;
    private const int TOO_LONG_LIMIT_MS = 500;

    public PerformanceBehaviour(ILogger<TRequest> logger, ICurrentUserService currentUserService)
    {
        timer = new Stopwatch();
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
    }

    public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
    {
        timer.Start();

        var response = await next();

        timer.Stop();

        var elapsedMilliseconds = timer.ElapsedMilliseconds;

        if (elapsedMilliseconds > TOO_LONG_LIMIT_MS)
        {
            var requestName = typeof(TRequest).Name;
            var userId = currentUserService.GetUser()?.GetUserId() ?? string.Empty;
            var userName = currentUserService.GetUser()?.GetUserName() ?? string.Empty;
            logger.LogWarning("Long Running Request: {Name} ({ElapsedMilliseconds} milliseconds) {UserId} {UserName} {Request}",
                requestName, elapsedMilliseconds, userId, userName, request);
        }

        return response;
    }
}
