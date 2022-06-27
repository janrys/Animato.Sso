namespace Animato.Sso.Infrastructure.AzureStorage.Services.Persistence.DTOs;
using Animato.Sso.Domain.Entities;
using Azure;
using Azure.Data.Tables;

public class ClaimTableEntity : ITableEntity
{
    public ClaimTableEntity()
    {

    }
    public ClaimTableEntity(ClaimId claimId, string name) : this(claimId.ToString(), name) { }
    public ClaimTableEntity(string claimId, string name)
    {
        Name = name;
        ClaimId = claimId;
    }

    public string ClaimId { get => RowKey; set => RowKey = value; }
    public string Name { get => PartitionKey; private set => PartitionKey = value; }

    public string PartitionKey { get; set; }
    public string RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    public string Description { get; set; }
}


public static class ClaimTableEntityExtensions
{
    public static Claim ToEntity(this ClaimTableEntity tableEntity)
     => new()
     {
         Id = new(Guid.Parse(tableEntity.ClaimId)),
         Name = tableEntity.Name,
         Description = tableEntity.Description
     };

    public static ClaimTableEntity ToTableEntity(this Claim claim)
     => new(claim.Id, claim.Name) { Description = claim.Description };
}
