namespace Animato.Sso.WebApi.Common;
using Animato.Sso.Application.Features.Users.DTOs;
using Animato.Sso.Application.Models;
using Animato.Sso.Domain.Entities;

public interface IUserCommandBuilder
{
    Task<AuthorizationResult> Authorize(AuthorizationRequest authorizationRequest);
    Task<User> Login(string login, string password);
    Task<TokenResult> GetToken(TokenRequest tokenRequest);
    Task<User> Create(CreateUserModel user);
    Task<User> Update(UserId userID, CreateUserModel user);
    Task Delete(UserId userID);
    Task<IEnumerable<ApplicationRole>> RemoveRole(UserId userId, ApplicationRoleId roleId);
    Task<IEnumerable<ApplicationRole>> AddRole(UserId userId, ApplicationRoleId roleId);
    Task<IEnumerable<ApplicationRole>> AddRoles(UserId userId, params ApplicationRoleId[] roleIds);
    Task<IEnumerable<UserClaim>> RemoveClaim(UserId userId, UserClaimId validUserClaimId);
    Task<IEnumerable<UserClaim>> AddClaims(UserId userId, AddUserClaimsModel claims);
    Task<IEnumerable<UserClaim>> UpdateClaim(UserId userId, UserClaimId validUserClaimId, UpdateUserClaimModel claim);
}
