namespace Animato.Sso.Application.Models;

using Animato.Sso.Domain.Enums;

public class TokenResult
{
    public bool IsAuthorized { get; set; }
    public GrantType GrantType { get; set; }
    public string AccessToken { get; set; }
    public int ExpiresIn { get; set; }
    public string RefreshToken { get; set; }
    public string IdToken { get; set; }
}
