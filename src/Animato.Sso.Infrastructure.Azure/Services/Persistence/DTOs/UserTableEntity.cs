namespace Animato.Sso.Infrastructure.AzureStorage.Services.Persistence.DTOs;
using Animato.Sso.Domain.Entities;
using Azure;
using Azure.Data.Tables;

public class UserTableEntity : ITableEntity
{
    public UserTableEntity()
    {

    }
    public UserTableEntity(string login, UserId id) : this(login, id.ToString()) { }
    public UserTableEntity(string login, string id)
    {
        Id = id;
        Login = login;
    }

    public string Id { get => RowKey; set => RowKey = value; }
    public string Login { get => PartitionKey; private set => PartitionKey = value.ToLowerInvariant(); }

    public string PartitionKey { get; set; }
    public string RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    public string Name { get; set; }
    public string FullName { get; set; }
    public string Salt { get; set; }
    public string Password { get; set; }
    public string PasswordHashAlgorithm { get; set; }
    public DateTime PasswordLastChanged { get; set; }
    public string TotpSecretKey { get; set; }
    public string AuthorizationMethod { get; set; }
    public DateTime LastChanged { get; set; }
    public bool IsDeleted { get; set; } = false;
    public bool IsBlocked { get; set; } = false;

}


public static class UserTableEntityExtensions
{
    public static User ToEntity(this UserTableEntity tableEntity)
     => new()
     {
         Id = new(Guid.Parse(tableEntity.Id)),
         Login = tableEntity.Login,
         Name = tableEntity.Name,
         FullName = tableEntity.FullName,
         Salt = tableEntity.Salt,
         Password = tableEntity.Password,
         TotpSecretKey = tableEntity.TotpSecretKey,
         AuthorizationMethod = Domain.Enums.AuthorizationMethod.FromName(tableEntity.AuthorizationMethod),
         LastChanged = tableEntity.LastChanged,
         IsDeleted = tableEntity.IsDeleted,
         IsBlocked = tableEntity.IsBlocked,
         PasswordHashAlgorithm = Domain.Enums.HashAlgorithmType.FromName(tableEntity.PasswordHashAlgorithm),
         PasswordLastChanged = tableEntity.PasswordLastChanged,
     };

    public static UserTableEntity ToTableEntity(this User user)
     => new(user.Login, user.Id)
     {
         Name = user.Name,
         FullName = user.FullName,
         Salt = user.Salt,
         Password = user.Password,
         TotpSecretKey = user.TotpSecretKey,
         AuthorizationMethod = user.AuthorizationMethod.Name,
         LastChanged = user.LastChanged,
         IsDeleted = user.IsDeleted,
         IsBlocked = user.IsBlocked,
         PasswordHashAlgorithm = user.PasswordHashAlgorithm.Name,
         PasswordLastChanged = user.PasswordLastChanged,
     };
}
