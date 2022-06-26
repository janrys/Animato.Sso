namespace Animato.Sso.WebApi.Common;

using Animato.Sso.Application.Features.Claims.DTOs;
using Animato.Sso.Domain.Entities;

public interface IClaimCommandBuilder
{
    Task<Claim> Create(CreateClaimModel claim);
    Task<Claim> Update(string oldName, CreateClaimModel claim);
    Task Delete(string name);
}
