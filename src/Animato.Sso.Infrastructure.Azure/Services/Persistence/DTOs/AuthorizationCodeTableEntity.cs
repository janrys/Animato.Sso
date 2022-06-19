namespace Animato.Sso.Infrastructure.AzureStorage.Services.Persistence.DTOs;
using Animato.Sso.Domain.Entities;
using Azure;
using Azure.Data.Tables;

public class AuthorizationCodeTableEntity : ITableEntity
{
    public AuthorizationCodeTableEntity()
    {

    }

    public AuthorizationCodeTableEntity(string code, string clientId)
    {
        Code = code;
        ClientId = clientId;
    }

    public string Code { get => RowKey; set => RowKey = value; }
    public string ClientId { get => PartitionKey; private set => PartitionKey = value; }

    public string PartitionKey { get; set; }
    public string RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }


    public string RedirectUri { get; set; }
    public string Scope { get; set; }
    public string UserId { get; set; }
    public string ApplicationId { get; set; }
    public DateTime Created { get; set; }
}


public static class AuthorizationCodeTableEntityExtensions
{
    public static AuthorizationCode ToEntity(this AuthorizationCodeTableEntity tableEntity)
     => new()
     {
         Code = tableEntity.Code,
         ClientId = tableEntity.ClientId,
         RedirectUri = tableEntity.RedirectUri,
         Scope = tableEntity.Scope,
         UserId = new(Guid.Parse(tableEntity.UserId)),
         ApplicationId = new(Guid.Parse(tableEntity.ApplicationId)),
         Created = tableEntity.Created
     };

    public static AuthorizationCodeTableEntity ToTableEntity(this AuthorizationCode code)
     => new(code.Code, code.ClientId)
     {
         RedirectUri = code.RedirectUri,
         Scope = code.Scope,
         UserId = code.UserId.ToString(),
         ApplicationId = code.ApplicationId.ToString(),
         Created = code.Created,
     };
}
