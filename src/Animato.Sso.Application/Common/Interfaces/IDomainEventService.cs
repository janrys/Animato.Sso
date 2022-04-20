namespace Animato.Sso.Application.Common.Interfaces;
using Animato.Sso.Domain.Common;

public interface IDomainEventService
{
    Task Publish(DomainEvent domainEvent, CancellationToken cancellationToken);
}
