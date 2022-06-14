namespace Animato.Sso.Application.Security;
using System.Security.Claims;
using Animato.Sso.Domain.Entities;

public static class ClaimsPrincipalExtensions
{
    public static string GetUserUpn(this ClaimsPrincipal principal)
        => principal.FindFirst(ClaimTypes.Upn)?.Value;

    public static string GetUserName(this ClaimsPrincipal principal)
        => principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;


    public static UserId GetUserId(this ClaimsPrincipal principal)
    {
        var claimUserId = principal.FindFirst(ClaimTypes.Sid)?.Value;

        if (string.IsNullOrEmpty(claimUserId) || !Guid.TryParse(claimUserId, out var userId))
        {
            return UserId.Empty;
        }

        return new UserId(userId);
    }
}
