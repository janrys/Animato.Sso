namespace Animato.Sso.Infrastructure.AzureStorage.Services.Persistence.DTOs;
using Animato.Sso.Domain.Entities;
using Azure;
using Azure.Data.Tables;

public class ScopeTableEntity : ITableEntity
{
    public ScopeTableEntity()
    {

    }
    public ScopeTableEntity(ScopeId id, string name) : this(id.ToString(), name) { }
    public ScopeTableEntity(string id, string name)
    {
        Id = id;
        Name = name;
    }

    public string Id { get => RowKey; set => RowKey = value; }
    public string Name { get => PartitionKey; private set => PartitionKey = value; }

    public string PartitionKey { get; set; }
    public string RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

}

public static class ScopeTableEntityExtensions
{
    public static Scope ToEntity(this ScopeTableEntity tableEntity)
     => new()
     {
         Id = new(Guid.Parse(tableEntity.Id)),
         Name = tableEntity.Name,
     };

    public static ScopeTableEntity ToTableEntity(this Scope scope)
     => new(scope.Id, scope.Name);
}
