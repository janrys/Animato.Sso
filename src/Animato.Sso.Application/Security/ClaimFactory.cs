namespace Animato.Sso.Application.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Animato.Sso.Application.Common;
using Animato.Sso.Application.Common.Interfaces;
using Animato.Sso.Domain.Entities;
using SecurityClaims = System.Security.Claims;

public class ClaimFactory : IClaimFactory
{
    public IEnumerable<SecurityClaims.Claim> GenerateClaims(User user, params ApplicationRole[] roles)
    {
        if (user is null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        var claims = new List<SecurityClaims.Claim>
        {
            new SecurityClaims.Claim(ClaimTypes.NameIdentifier, user.Login),
            new SecurityClaims.Claim(ClaimTypes.Name, user.Login),
            new SecurityClaims.Claim("name", user.Name),
            new SecurityClaims.Claim("full_name", user.FullName),
            new SecurityClaims.Claim(ClaimTypes.Sid, user.Id.Value.ToString()),
            new SecurityClaims.Claim("last_changed", user.LastChanged.ToUniversalTime().ToString(DefaultOptions.DatePattern, DefaultOptions.Culture))
        };

        if (roles is not null && roles.Any())
        {
            claims.AddRange(roles.Select(r => new SecurityClaims.Claim(ClaimTypes.Role, r.Name)));

        }

        return claims;
    }
}
