namespace Animato.Sso.Domain.Entities;

public class AssetMetadata
{
    public AssetMetadata() : this(Guid.NewGuid()) { }

    public AssetMetadata(Guid id) => Id = id;

    public Guid Id { get; set; }
    public string ContentType { get; set; }
    public List<AssetVariant> Variants { get; set; } = new List<AssetVariant>();
}
