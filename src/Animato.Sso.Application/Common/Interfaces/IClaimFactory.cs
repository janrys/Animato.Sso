namespace Animato.Sso.Application.Common.Interfaces;
using System.Collections.Generic;
using Animato.Sso.Domain.Entities;
using Animato.Sso.Domain.Enums;

public interface IClaimFactory
{
    public IEnumerable<System.Security.Claims.Claim> GenerateClaims(User user, AuthorizationMethod authorizationMethod, params ApplicationRole[] roles);
    public IEnumerable<System.Security.Claims.Claim> GenerateClaims(User user, IEnumerable<AuthorizationMethod> authorizationMethods, params ApplicationRole[] roles);
}
