namespace Animato.Sso.Domain.Common;

public abstract class DomainEvent
{
    public bool IsPublished { get; set; } = false;
    public DateTimeOffset Occurred { get; set; } = DateTime.UtcNow;
    public string Version { get; protected set; } = DomainEventVersions.VERSION_1;
}
