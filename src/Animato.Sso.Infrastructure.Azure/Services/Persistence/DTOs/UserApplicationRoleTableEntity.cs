namespace Animato.Sso.Infrastructure.AzureStorage.Services.Persistence.DTOs;
using Animato.Sso.Domain.Entities;
using Azure;
using Azure.Data.Tables;

public class UserApplicationRoleTableEntity : ITableEntity
{
    public UserApplicationRoleTableEntity()
    {

    }
    public UserApplicationRoleTableEntity(ApplicationRoleId applicationRoleId, UserId userId) : this(applicationRoleId.ToString(), userId.ToString()) { }
    public UserApplicationRoleTableEntity(string applicationRoleId, string userId)
    {
        UserId = userId;
        ApplicationRoleId = applicationRoleId;
    }

    public string UserId { get => RowKey; set => RowKey = value; }
    public string ApplicationRoleId { get => PartitionKey; private set => PartitionKey = value; }

    public string PartitionKey { get; set; }
    public string RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}


public static class UserApplicationRoleTableEntityExtensions
{
    public static UserApplicationRole ToEntity(this UserApplicationRoleTableEntity tableEntity)
     => new()
     {
         UserId = new(Guid.Parse(tableEntity.UserId)),
         ApplicationRoleId = new(Guid.Parse(tableEntity.ApplicationRoleId))
     };

    public static UserApplicationRoleTableEntity ToTableEntity(this UserApplicationRole role)
     => new(role.ApplicationRoleId, role.UserId);
}
