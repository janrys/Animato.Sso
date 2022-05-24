namespace Animato.Sso.Domain.Entities;
public class AuthorizationCode
{
    public string Code { get; set; }
    public string ClientId { get; set; }
    public string RedirectUri { get; set; }
    public string Scope { get; set; }
    public UserId UserId { get; set; }
    public ApplicationId ApplicationId { get; set; }
    public DateTime Created { get; set; } = DateTime.UtcNow;
}
