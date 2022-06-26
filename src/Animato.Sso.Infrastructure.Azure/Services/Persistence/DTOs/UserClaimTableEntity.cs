namespace Animato.Sso.Infrastructure.AzureStorage.Services.Persistence.DTOs;
using Animato.Sso.Domain.Entities;
using Azure;
using Azure.Data.Tables;

public class UserClaimTableEntity : ITableEntity
{
    public UserClaimTableEntity()
    {

    }
    public UserClaimTableEntity(UserId userId, ClaimId claimId) : this(userId.ToString(), claimId.ToString()) { }
    public UserClaimTableEntity(string userId, string claimId)
    {
        UserId = userId;
        ClaimId = claimId;
    }

    public string ClaimId { get => RowKey; set => RowKey = value; }
    public string UserId { get => PartitionKey; private set => PartitionKey = value; }

    public string PartitionKey { get; set; }
    public string RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    public string Value { get; set; }
}


public static class UserClaimTableEntityExtensions
{
    public static UserClaim ToEntity(this UserClaimTableEntity tableEntity)
     => new()
     {
         UserId = new(Guid.Parse(tableEntity.UserId)),
         ClaimId = new(Guid.Parse(tableEntity.ClaimId))
     };

    public static UserClaimTableEntity ToTableEntity(this UserClaim claim)
     => new(claim.UserId, claim.ClaimId);
}
