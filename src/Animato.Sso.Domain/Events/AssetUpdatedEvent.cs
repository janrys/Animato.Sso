namespace Animato.Sso.Domain.Events;

using Animato.Sso.Domain.Common;
using Animato.Sso.Domain.Entities;

public class AssetUpdatedEvent : DomainEvent
{
    public AssetUpdatedEvent(AssetMetadata asset) => Asset = asset ?? throw new ArgumentNullException(nameof(asset));

    public AssetMetadata Asset { get; }
}
