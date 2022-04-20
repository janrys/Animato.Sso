namespace Animato.Sso.Infrastructure.Services.Persistence;
using System.Collections.Generic;
using System.Web;
using Animato.Sso.Domain.Entities;
using Azure;
using Azure.Data.Tables;

public class AssetMetadataTableEntity : ITableEntity
{
    public AssetMetadataTableEntity()
    {

    }
    public AssetMetadataTableEntity(string contentType, Guid id, List<AssetVariant> variants) : this(contentType, id) => SetVariants(variants);
    public AssetMetadataTableEntity(string contentType, Guid id) : this(contentType, id.ToString()) { }
    public AssetMetadataTableEntity(string contentType, string id)
    {
        ContentType = contentType;
        Id = id;
    }

    public string ContentType { get => HttpUtility.UrlDecode(PartitionKey); set => PartitionKey = HttpUtility.UrlEncode(value); }
    public string Id { get => RowKey; set => RowKey = value; }
    public string Variants { get; set; }
    public string PartitionKey { get; set; }
    public string RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    public List<AssetVariant> GetVariants() => Newtonsoft.Json.JsonConvert.DeserializeObject<List<AssetVariant>>(Variants);

    public void SetVariants(List<AssetVariant> variants) => Variants = Newtonsoft.Json.JsonConvert.SerializeObject(variants);

}


public static class AssetMetadataTableEntityExtensions
{
    public static AssetMetadata ToAssetMetadata(this AssetMetadataTableEntity tableEntity) => new()
    {
        Id = Guid.Parse(tableEntity.Id),
        ContentType = tableEntity.ContentType,
        Variants = tableEntity.GetVariants()
    };
}
