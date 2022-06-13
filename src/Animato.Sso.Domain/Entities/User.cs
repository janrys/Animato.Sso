namespace Animato.Sso.Domain.Entities;

using Animato.Sso.Domain.Enums;

public class User
{
    public static readonly User EmptyUser = new() { Id = new UserId(Guid.Parse("00000001 - 0000 - 0000 - 0000 - 000000000001")) };
    public static readonly User SystemUser = new() { Id = new UserId(Guid.Parse("00000002 - 0000 - 0000 - 0000 - 000000000002")) };

    public UserId Id { get; set; }
    public string Name { get; set; }
    public string FullName { get; set; }
    public string Login { get; set; }
    public string Salt { get; set; }
    public string Password { get; set; }
    public AuthorizationType AuthorizationType { get; set; }
    public DateTime LastChanged { get; set; }
    public bool IsDeleted { get; set; } = false;
    public bool IsBlocked { get; set; } = false;
}

