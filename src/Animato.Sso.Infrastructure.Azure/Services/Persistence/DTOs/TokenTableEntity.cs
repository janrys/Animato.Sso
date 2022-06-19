namespace Animato.Sso.Infrastructure.AzureStorage.Services.Persistence.DTOs;
using Animato.Sso.Domain.Entities;
using Azure;
using Azure.Data.Tables;

public class TokenTableEntity : ITableEntity
{
    public TokenTableEntity()
    {

    }
    public TokenTableEntity(UserId userId, TokenId id) : this(userId.ToString(), id.ToString()) { }
    public TokenTableEntity(string userId, string id)
    {
        Id = id;
        UserId = userId;
    }

    public string Id { get => RowKey; set => RowKey = value; }
    public string UserId { get => PartitionKey; private set => PartitionKey = value; }

    public string PartitionKey { get; set; }
    public string RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }


    public string TokenType { get; set; }
    public string ApplicationId { get; set; }
    public string RefreshTokenId { get; set; }
    public string Value { get; set; }
    public DateTime Created { get; set; }
    public DateTime Expiration { get; set; }
    public DateTime? Revoked { get; set; }

}


public static class TokenTableEntityExtensions
{
    public static Token ToEntity(this TokenTableEntity tableEntity)
     => new()
     {
         Id = new(Guid.Parse(tableEntity.Id)),
         ApplicationId = new(Guid.Parse(tableEntity.ApplicationId)),
         TokenType = Domain.Enums.TokenType.FromName(tableEntity.TokenType),
         UserId = new(Guid.Parse(tableEntity.UserId)),
         RefreshTokenId = string.IsNullOrEmpty(tableEntity.RefreshTokenId) ? null : new(Guid.Parse(tableEntity.RefreshTokenId)),
         Value = tableEntity.Value,
         Created = tableEntity.Created,
         Expiration = tableEntity.Expiration,
         Revoked = tableEntity.Revoked
     };

    public static TokenTableEntity ToTableEntity(this Token token)
     => new(token.UserId, token.Id)
     {
         TokenType = token.TokenType.Name,
         ApplicationId = token.ApplicationId.ToString(),
         RefreshTokenId = token.RefreshTokenId?.ToString(),
         Value = token.Value,
         Created = token.Created,
         Expiration = token.Expiration,
         Revoked = token.Revoked
     };
}
