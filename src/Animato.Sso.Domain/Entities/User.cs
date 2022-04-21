namespace Animato.Sso.Domain.Entities;
public class User
{
    public UserId Id { get; set; }
    public string Name { get; set; }
    public string Login { get; set; }
    public string Salt { get; set; }
    public string Password { get; set; }
}

