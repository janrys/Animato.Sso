namespace Animato.Sso.Domain.Events;

using Animato.Sso.Domain.Common;
using Animato.Sso.Domain.Entities;

public class AssetDeletedEvent : DomainEvent
{
    public AssetDeletedEvent(AssetMetadata asset) => Asset = asset ?? throw new ArgumentNullException(nameof(asset));

    public AssetMetadata Asset { get; }
}
