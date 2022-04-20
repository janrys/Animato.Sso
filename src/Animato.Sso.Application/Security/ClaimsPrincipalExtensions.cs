namespace Animato.Sso.Application.Security;
using System.Security.Claims;

public static class ClaimsPrincipalExtensions
{
    public static string GetUserId(this ClaimsPrincipal principal)
        => principal.FindFirst(ClaimTypes.Upn)?.Value;

    public static string GetUserName(this ClaimsPrincipal principal)
        => principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
}
