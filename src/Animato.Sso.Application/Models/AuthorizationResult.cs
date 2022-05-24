namespace Animato.Sso.Application.Models;

using Animato.Sso.Domain.Enums;

public class AuthorizationResult
{
    public bool IsAuthorized { get; set; }
    public AuthorizationFlowType AuthorizationFlow { get; set; }
    public string AccessToken { get; set; }
    public string Code { get; set; }
}
