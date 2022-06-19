namespace Animato.Sso.Infrastructure.AzureStorage.Services.Persistence.DTOs;
using Animato.Sso.Domain.Entities;
using Azure;
using Azure.Data.Tables;

public class ApplicationRoleTableEntity : ITableEntity
{
    public ApplicationRoleTableEntity()
    {

    }
    public ApplicationRoleTableEntity(ApplicationId applicationId, ApplicationRoleId id) : this(applicationId.ToString(), id.ToString()) { }
    public ApplicationRoleTableEntity(string applicationId, string id)
    {
        Id = id;
        ApplicationId = applicationId;
    }

    public string Id { get => RowKey; set => RowKey = value; }
    public string ApplicationId { get => PartitionKey; private set => PartitionKey = value; }

    public string PartitionKey { get; set; }
    public string RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    public string Name { get; set; }

}


public static class ApplicationRoleTableEntityExtensions
{
    public static ApplicationRole ToEntity(this ApplicationRoleTableEntity tableEntity)
     => new()
     {
         Id = new(Guid.Parse(tableEntity.Id)),
         ApplicationId = new(Guid.Parse(tableEntity.ApplicationId)),
         Name = tableEntity.Name
     };

    public static ApplicationRoleTableEntity ToTableEntity(this ApplicationRole role)
     => new(role.ApplicationId, role.Id)
     {
         Name = role.Name
     };
}
