namespace Animato.Sso.Application.Features.Users.DTOs;

using Animato.Sso.Domain.Enums;

public class CreateUserModel
{
    public string Name { get; set; }
    public string FullName { get; set; }
    public string Login { get; set; }
    public string Password { get; set; }
    public string TotpSecretKey { get; set; }
    public AuthorizationMethod AuthorizationMethod { get; set; }
    public bool? IsDeleted { get; set; } = false;
    public bool? IsBlocked { get; set; } = false;
}
