namespace Animato.Sso.Domain.Entities;
public class User
{
    public UserId Id { get; set; }
    public string Name { get; set; }
    public string FullName { get; set; }
    public string Login { get; set; }
    public string Salt { get; set; }
    public string Password { get; set; }
    public bool Use2FA { get; set; }
    public DateTime LastChanged { get; set; }
}

