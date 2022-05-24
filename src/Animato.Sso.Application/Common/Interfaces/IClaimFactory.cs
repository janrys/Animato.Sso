namespace Animato.Sso.Application.Common.Interfaces;
using System.Collections.Generic;
using Animato.Sso.Domain.Entities;

public interface IClaimFactory
{
    public IEnumerable<System.Security.Claims.Claim> GenerateClaims(User user, params ApplicationRole[] roles);
}
