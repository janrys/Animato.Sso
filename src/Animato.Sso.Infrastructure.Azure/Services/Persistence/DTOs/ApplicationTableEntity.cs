namespace Animato.Sso.Infrastructure.AzureStorage.Services.Persistence.DTOs;
using Animato.Sso.Domain.Entities;
using Azure;
using Azure.Data.Tables;

public class ApplicationTableEntity : ITableEntity
{
    public ApplicationTableEntity()
    {

    }
    public ApplicationTableEntity(ApplicationId id, string code) : this(id.ToString(), code) { }
    public ApplicationTableEntity(string id, string code)
    {
        Id = id;
        Code = code;
    }

    public string Id { get => RowKey; private set => RowKey = value; }
    public string Code { get => PartitionKey; private set => PartitionKey = value; }
    public string PartitionKey { get; set; }
    public string RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    public string Name { get; set; }
    public string Secrets { get; set; }
    public string RedirectUris { get; set; }
    public int AccessTokenExpirationMinutes { get; set; }
    public int RefreshTokenExpirationMinutes { get; set; }
    public string AuthorizationMethod { get; set; }

}


public static class ApplicationTableEntityExtensions
{
    public static Application ToEntity(this ApplicationTableEntity tableEntity)
     => new()
     {
         Id = new(Guid.Parse(tableEntity.Id)),
         Code = tableEntity.Code,
         Name = tableEntity.Name,
         Secrets = string.IsNullOrEmpty(tableEntity.Secrets)
            ? new List<string>()
            : new List<string>(tableEntity.Secrets.Split(AzureTableStorageOptions.ArraySplitter)),
         RedirectUris = string.IsNullOrEmpty(tableEntity.RedirectUris)
            ? new List<string>()
            : new List<string>(tableEntity.RedirectUris.Split(AzureTableStorageOptions.ArraySplitter)),
         AccessTokenExpirationMinutes = tableEntity.AccessTokenExpirationMinutes,
         RefreshTokenExpirationMinutes = tableEntity.RefreshTokenExpirationMinutes,
         AuthorizationMethod = Domain.Enums.AuthorizationMethod.FromName(tableEntity.AuthorizationMethod),
     };

    public static ApplicationTableEntity ToTableEntity(this Application application)
     => new(application.Id, application.Code)
     {
         Name = application.Name,
         Secrets = application.Secrets == null || !application.Secrets.Any()
            ? ""
            : string.Join(AzureTableStorageOptions.ArraySplitter, application.Secrets),
         RedirectUris = application.RedirectUris == null || !application.RedirectUris.Any()
            ? ""
            : string.Join(AzureTableStorageOptions.ArraySplitter, application.RedirectUris),
         AccessTokenExpirationMinutes = application.AccessTokenExpirationMinutes,
         RefreshTokenExpirationMinutes = application.RefreshTokenExpirationMinutes,
         AuthorizationMethod = application.AuthorizationMethod.Name,
     };
}
