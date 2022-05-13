namespace Animato.Sso.Infrastructure.Services.Persistence;
using Animato.Sso.Domain.Entities;
using Azure;
using Azure.Data.Tables;

public class TransformationDefinitionTableEntity : ITableEntity
{
    public TransformationDefinitionTableEntity()
    {

    }
    public TransformationDefinitionTableEntity(Guid id) : this(id.ToString()) { }
    public TransformationDefinitionTableEntity(string id)
    {
        Id = id;
        PartitionKey = "definition";
    }

    public string Id { get => RowKey; set => RowKey = value; }
    public string Definition { get; set; }
    public string Description { get; set; }
    public string PartitionKey { get; set; }
    public string RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

}


public static class TransformationDefinitionTableEntityExtensions
{
    public static TransformationDefinition ToTransformationDefinition(this TransformationDefinitionTableEntity tableEntity) => new()
    {
        Id = Guid.Parse(tableEntity.Id),
        Definition = tableEntity.Definition,
        Description = tableEntity.Description
    };
}
