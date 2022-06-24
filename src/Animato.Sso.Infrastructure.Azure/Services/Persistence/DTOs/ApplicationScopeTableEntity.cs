namespace Animato.Sso.Infrastructure.AzureStorage.Services.Persistence.DTOs;
using Animato.Sso.Domain.Entities;
using Azure;
using Azure.Data.Tables;

public class ApplicationScopeTableEntity : ITableEntity
{
    public ApplicationScopeTableEntity()
    {

    }
    public ApplicationScopeTableEntity(ApplicationId applicationId, ScopeId scopeId) : this(applicationId.ToString(), scopeId.ToString()) { }
    public ApplicationScopeTableEntity(string applicationId, string scopeId)
    {
        ScopeId = scopeId;
        ApplicationId = applicationId;
    }

    public string ScopeId { get => RowKey; set => RowKey = value; }
    public string ApplicationId { get => PartitionKey; private set => PartitionKey = value; }

    public string PartitionKey { get; set; }
    public string RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

}

public static class ApplicationScopeTableEntityExtensions
{
    public static ApplicationScope ToEntity(this ApplicationScopeTableEntity tableEntity)
     => new()
     {
         ScopeId = new(Guid.Parse(tableEntity.ScopeId)),
         ApplicationId = new(Guid.Parse(tableEntity.ApplicationId))
     };

    public static ApplicationScopeTableEntity ToTableEntity(this ApplicationScope scope)
     => new(scope.ApplicationId, scope.ScopeId);
}
