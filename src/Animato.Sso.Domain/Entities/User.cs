namespace Animato.Sso.Domain.Entities;

using Animato.Sso.Domain.Enums;

public class User
{
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

