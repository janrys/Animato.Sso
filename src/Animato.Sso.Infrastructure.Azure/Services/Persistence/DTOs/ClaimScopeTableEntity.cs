namespace Animato.Sso.Infrastructure.AzureStorage.Services.Persistence.DTOs;
using Animato.Sso.Domain.Entities;
using Azure;
using Azure.Data.Tables;

public class ClaimScopeTableEntity : ITableEntity
{
    public ClaimScopeTableEntity()
    {

    }
    public ClaimScopeTableEntity(ClaimId claimId, ScopeId scopeId) : this(claimId.ToString(), scopeId.ToString()) { }
    public ClaimScopeTableEntity(string claimId, string scopeId)
    {
        ScopeId = scopeId;
        ClaimId = claimId;
    }

    public string ClaimId { get => RowKey; set => RowKey = value; }
    public string ScopeId { get => PartitionKey; private set => PartitionKey = value; }

    public string PartitionKey { get; set; }
    public string RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    public string Description { get; set; }
}


public static class ClaimScopeTableEntityExtensions
{
    public static ClaimScope ToEntity(this ClaimScopeTableEntity tableEntity)
     => new()
     {
         ClaimId = new(Guid.Parse(tableEntity.ClaimId)),
         ScopeId = new(Guid.Parse(tableEntity.ScopeId)),
     };

    public static ClaimScopeTableEntity ToTableEntity(this ClaimScope claim)
     => new(claim.ClaimId, claim.ScopeId);
}
