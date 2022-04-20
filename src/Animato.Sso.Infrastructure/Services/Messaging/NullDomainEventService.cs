namespace Animato.Sso.Infrastructure.Services.Messaging;
using System.Threading;
using System.Threading.Tasks;
using Animato.Sso.Application.Common.Interfaces;
using Animato.Sso.Domain.Common;

public class NullDomainEventService : IDomainEventService
{
    public Task Publish(DomainEvent domainEvent, CancellationToken cancellationToken) => Task.CompletedTask;
}
