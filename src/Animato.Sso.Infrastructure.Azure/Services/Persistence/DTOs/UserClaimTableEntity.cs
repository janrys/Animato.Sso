namespace Animato.Sso.Infrastructure.AzureStorage.Services.Persistence.DTOs;
using Animato.Sso.Domain.Entities;
using Azure;
using Azure.Data.Tables;

public class UserClaimTableEntity : ITableEntity
{
    public UserClaimTableEntity()
    {

    }
    public UserClaimTableEntity(UserId userId, UserClaimId id) : this(userId.ToString(), id.ToString()) { }
    public UserClaimTableEntity(string userId, string id)
    {
        UserId = userId;
        Id = id;
    }

    public string Id { get => RowKey; set => RowKey = value; }
    public string UserId { get => PartitionKey; private set => PartitionKey = value; }

    public string PartitionKey { get; set; }
    public string RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    public string ClaimId { get; set; }
    public string Value { get; set; }
}


public static class UserClaimTableEntityExtensions
{
    public static UserClaim ToEntity(this UserClaimTableEntity tableEntity)
     => new()
     {
         Id = new(Guid.Parse(tableEntity.Id)),
         UserId = new(Guid.Parse(tableEntity.UserId)),
         ClaimId = new(Guid.Parse(tableEntity.ClaimId)),
         Value = tableEntity.Value
     };

    public static UserClaimTableEntity ToTableEntity(this UserClaim claim)
     => new(claim.UserId, claim.Id)
     {
         ClaimId = claim.ClaimId.ToString(),
         Value = claim.Value
     };
}
