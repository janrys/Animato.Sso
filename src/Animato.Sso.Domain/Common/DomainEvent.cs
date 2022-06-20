namespace Animato.Sso.Domain.Common;

public abstract class DomainEvent
{
    public DomainEvent(DateTimeOffset occurred) => Occurred = occurred;

    public bool IsPublished { get; set; } = false;
    public DateTimeOffset Occurred { get; set; }
    public string Version { get; protected set; } = DomainEventVersions.VERSION_1;
}
