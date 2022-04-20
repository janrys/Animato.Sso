namespace Animato.Sso.Infrastructure.Services.Messaging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Animato.Sso.Application.Common.Interfaces;
using Animato.Sso.Application.Common.Logging;
using Animato.Sso.Domain.Common;
using Microsoft.Extensions.Logging;

public class LoggingDomainEventService : IDomainEventService
{
    private readonly IDomainEventService innerEventService;
    private readonly ILogger<LoggingDomainEventService> logger;

    public LoggingDomainEventService(IDomainEventService innerEventService, ILogger<LoggingDomainEventService> logger)
    {
        this.innerEventService = innerEventService ?? throw new ArgumentNullException(nameof(innerEventService));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Publish(DomainEvent domainEvent, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Publishing event {EventType}, data {EventData}", domainEvent.GetType().Name, domainEvent.ToLogString());
            await innerEventService.Publish(domainEvent, cancellationToken);
            logger.LogDebug("Published event {EventType}, data {EventData}", domainEvent.GetType().Name, domainEvent.ToLogString());
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error publishing on {EventServiceType} event {EventType}, data {EventData}",
                innerEventService.GetType().Name, domainEvent.GetType().Name, domainEvent.ToLogString());
        }
    }
}
