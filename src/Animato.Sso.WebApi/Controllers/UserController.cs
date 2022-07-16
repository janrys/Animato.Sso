namespace Animato.Sso.WebApi.Controllers;
using Animato.Sso.Application.Features.Users.DTOs;
using Animato.Sso.WebApi.Extensions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status500InternalServerError)]

public class UserController : ApiControllerBase
{
    public UserController(ISender mediator) : base(mediator) { }

    /// <summary>
    /// Get all users
    /// </summary>
    /// <param name="login">User login</param>
    /// <param name="cancellationToken">Cancelation token</param>
    /// <returns>List of users</returns>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Domain.Entities.User>))]
    [HttpGet(Name = "GetUsers")]
    public async Task<IActionResult> GetAll([FromQuery] string login, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(login))
        {
            return await GetByLogin(login, cancellationToken);
        }

        var users = await this.QueryForCurrentUser(cancellationToken).User.GetAll();
        return Ok(users);
    }

    /// <summary>
    /// Get user
    /// </summary>
    /// <param name="id">User id</param>
    /// <param name="cancellationToken">Cancelation token</param>
    /// <returns>User</returns>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Domain.Entities.User))]
    [HttpGet("{id}", Name = "GetUserById")]
    public async Task<IActionResult> GetById(string id, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(id))
        {
            return BadRequest($"{nameof(id)} must have a value");
        }

        Domain.Entities.UserId userId;
        if (Guid.TryParse(id, out var parsedUserId))
        {
            userId = new Domain.Entities.UserId(parsedUserId);
        }
        else
        {
            return BadRequest($"{nameof(id)} has a wrong format '{id}'");
        }

        var user = await this.QueryForCurrentUser(cancellationToken).User.GetById(userId);

        if (user is null)
        {
            return NotFound();
        }

        return Ok(user);
    }

    private async Task<IActionResult> GetByLogin([FromQuery] string login, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(login))
        {
            return BadRequest($"{nameof(login)} must have a value");
        }

        var user = await this.QueryForCurrentUser(cancellationToken).User.GetByLogin(login);

        if (user is null)
        {
            return NotFound();
        }

        return Ok(new List<Domain.Entities.User>() { user });
    }

    /// <summary>
    /// Create user
    /// </summary>
    /// <param name="user"></param>
    /// <param name="cancellationToken">Cancelation token</param>
    /// <returns>Created user</returns>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Domain.Entities.User))]
    [HttpPost(Name = "CreateUser")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserModel user, CancellationToken cancellationToken)
    {
        if (user is null)
        {
            return BadRequest($"{nameof(user)} must have a value");
        }

        var createdUser = await this.CommandForCurrentUser(cancellationToken).User.Create(user);
        return Ok(createdUser);
    }

    /// <summary>
    /// Update user
    /// </summary>
    /// <param name="id">User id to update</param>
    /// <param name="user">User changes</param>
    /// <param name="cancellationToken">Cancelation token</param>
    /// <returns>Updated user</returns>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Domain.Entities.User))]
    [HttpPut("{id}", Name = "UpdateUser")]
    public async Task<IActionResult> UpdateUser(string id, [FromBody] CreateUserModel user, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(id))
        {
            return BadRequest($"{nameof(id)} must have a value");
        }

        Domain.Entities.UserId userId;
        if (Guid.TryParse(id, out var parsedUserId))
        {
            userId = new Domain.Entities.UserId(parsedUserId);
        }
        else
        {
            return BadRequest($"{nameof(id)} has a wrong format '{id}'");
        }

        if (user is null)
        {
            return BadRequest($"{nameof(user)} must have a value");
        }

        var createdApplication = await this.CommandForCurrentUser(cancellationToken).User.Update(userId, user);
        return Ok(createdApplication);
    }

    /// <summary>
    /// Delete user
    /// </summary>
    /// <param name="id">User id to delete</param>
    /// <param name="cancellationToken">Cancelation token</param>
    /// <returns></returns>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [HttpDelete("{id}", Name = "DeleteUser")]
    public async Task<IActionResult> DeleteUser(string id, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(id))
        {
            return BadRequest($"{nameof(id)} must have a value");
        }

        Domain.Entities.UserId userId;
        if (Guid.TryParse(id, out var parsedUserId))
        {
            userId = new Domain.Entities.UserId(parsedUserId);
        }
        else
        {
            return BadRequest($"{nameof(id)} has a wrong format '{id}'");
        }

        await this.CommandForCurrentUser(cancellationToken).User.Delete(userId);
        return Ok();
    }

    /// <summary>
    /// Assign application roles to user
    /// </summary>
    /// <param name="id">User identifier</param>
    /// <param name="roles">Application user role identifiers</param>
    /// <param name="cancellationToken">Cancelation token</param>
    /// <returns>User application roles</returns>
    [HttpPost("{id}/role", Name = "AssignUserRoles")]
    public async Task<IActionResult> AssignUserRoles(string id, [FromBody] AddUserRolesModel roles, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(id))
        {
            return BadRequest($"{nameof(id)} must have a value");
        }

        if (roles is null || roles.RoleIds is null || !roles.RoleIds.Any(r => !string.IsNullOrEmpty(r)))
        {
            return BadRequest($"{nameof(roles)} must have a value");
        }

        Domain.Entities.UserId userId;
        if (Guid.TryParse(id, out var parsedUserId))
        {
            userId = new Domain.Entities.UserId(parsedUserId);
        }
        else
        {
            return BadRequest($"{nameof(id)} has a wrong format '{id}'");
        }

        var roleIds = new List<Domain.Entities.ApplicationRoleId>();
        foreach (var roleId in roles.RoleIds)
        {
            Domain.Entities.ApplicationRoleId applicationRoleId;
            if (Guid.TryParse(roleId, out var parsedApplicationRoleId))
            {
                applicationRoleId = new Domain.Entities.ApplicationRoleId(parsedApplicationRoleId);
                roleIds.Add(applicationRoleId);
            }
            else
            {
                return BadRequest($"{nameof(roleId)} has a wrong format '{roleId}'");
            }
        }

        var rolesResult = await this.CommandForCurrentUser(cancellationToken).User.AddRoles(userId, roleIds.ToArray());
        return Ok(rolesResult);
    }

    /// <summary>
    /// Assign application role to user
    /// </summary>
    /// <param name="id">User identifier</param>
    /// <param name="roleId">Application user role identifier</param>
    /// <param name="cancellationToken">Cancelation token</param>
    /// <returns>User application roles</returns>
    [HttpPost("{id}/role/{roleId}", Name = "AssignUserRole")]
    public async Task<IActionResult> AssignUserRole(string id, string roleId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(id))
        {
            return BadRequest($"{nameof(id)} must have a value");
        }

        if (string.IsNullOrEmpty(roleId))
        {
            return BadRequest($"{nameof(roleId)} must have a value");
        }

        Domain.Entities.UserId userId;
        if (Guid.TryParse(id, out var parsedUserId))
        {
            userId = new Domain.Entities.UserId(parsedUserId);
        }
        else
        {
            return BadRequest($"{nameof(id)} has a wrong format '{id}'");
        }

        Domain.Entities.ApplicationRoleId applicationRoleId;
        if (Guid.TryParse(roleId, out var parsedApplicationRoleId))
        {
            applicationRoleId = new Domain.Entities.ApplicationRoleId(parsedApplicationRoleId);
        }
        else
        {
            return BadRequest($"{nameof(roleId)} has a wrong format '{roleId}'");
        }

        var roles = await this.CommandForCurrentUser(cancellationToken).User.AddRole(userId, applicationRoleId);
        return Ok(roles);
    }

    /// <summary>
    /// Get user roles
    /// </summary>
    /// <param name="id">User id</param>
    /// <param name="cancellationToken">Cancelation token</param>
    /// <returns>User</returns>
    [HttpGet("{id}/role", Name = "GetUserRoles")]
    public async Task<IActionResult> GetUserRoles(string id, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(id))
        {
            return BadRequest($"{nameof(id)} must have a value");
        }

        Domain.Entities.UserId userId;
        if (Guid.TryParse(id, out var parsedUserId))
        {
            userId = new Domain.Entities.UserId(parsedUserId);
        }
        else
        {
            return BadRequest($"{nameof(id)} has a wrong format '{id}'");
        }

        var roles = await this.QueryForCurrentUser(cancellationToken).User.GetRoles(userId);
        return Ok(roles);
    }

    /// <summary>
    /// Unasign application role from user
    /// </summary>
    /// <param name="id">User identifier</param>
    /// <param name="roleId">Application role id to unassign</param>
    /// <param name="cancellationToken">Cancelation token</param>
    /// <returns>User application roles</returns>
    [HttpDelete("{id}/role/{roleId}", Name = "UnassignUserRole")]
    public async Task<IActionResult> UnassignUserRole(string id, string roleId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(id))
        {
            return BadRequest($"{nameof(id)} must have a value");
        }

        if (string.IsNullOrEmpty(roleId))
        {
            return BadRequest($"{nameof(roleId)} must have a value");
        }

        Domain.Entities.UserId userId;
        if (Guid.TryParse(id, out var parsedUserId))
        {
            userId = new Domain.Entities.UserId(parsedUserId);
        }
        else
        {
            return BadRequest($"{nameof(id)} has a wrong format '{id}'");
        }

        Domain.Entities.ApplicationRoleId applicationRoleId;
        if (Guid.TryParse(roleId, out var parsedApplicationRoleId))
        {
            applicationRoleId = new Domain.Entities.ApplicationRoleId(parsedApplicationRoleId);
        }
        else
        {
            return BadRequest($"{nameof(roleId)} has a wrong format '{roleId}'");
        }

        var roles = await this.CommandForCurrentUser(cancellationToken).User.RemoveRole(userId, applicationRoleId);
        return Ok(roles);
    }

    /// <summary>
    /// Add claims to user
    /// </summary>
    /// <param name="id">User identifier</param>
    /// <param name="claims">Claims with values</param>
    /// <param name="cancellationToken">Cancelation token</param>
    /// <returns>Added user claims</returns>
    [HttpPost("{id}/claim", Name = nameof(AddUserClaims))]
    public async Task<IActionResult> AddUserClaims(string id, [FromBody] AddUserClaimsModel claims, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(id))
        {
            return BadRequest($"{nameof(id)} must have a value");
        }

        if (claims is null || claims.Claims is null || !claims.Claims.Any(c => !string.IsNullOrEmpty(c.Name)))
        {
            return BadRequest($"{nameof(claims)} must have a value");
        }

        Domain.Entities.UserId userId;
        if (Guid.TryParse(id, out var parsedUserId))
        {
            userId = new Domain.Entities.UserId(parsedUserId);
        }
        else
        {
            return BadRequest($"{nameof(id)} has a wrong format '{id}'");
        }

        var userClaims = await this.CommandForCurrentUser(cancellationToken).User.AddClaims(userId, claims);
        return Ok(userClaims);
    }

    /// <summary>
    /// Get user claims
    /// </summary>
    /// <param name="id">User id</param>
    /// <param name="cancellationToken">Cancelation token</param>
    /// <returns>User claims</returns>
    [HttpGet("{id}/claim", Name = nameof(GetUserClaims))]
    public async Task<IActionResult> GetUserClaims(string id, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(id))
        {
            return BadRequest($"{nameof(id)} must have a value");
        }

        Domain.Entities.UserId userId;
        if (Guid.TryParse(id, out var parsedUserId))
        {
            userId = new Domain.Entities.UserId(parsedUserId);
        }
        else
        {
            return BadRequest($"{nameof(id)} has a wrong format '{id}'");
        }

        var claims = await this.QueryForCurrentUser(cancellationToken).User.GetClaims(userId);
        return Ok(claims);
    }

    /// <summary>
    /// Update user claim
    /// </summary>
    /// <param name="id">User identifier</param>
    /// <param name="userClaimId">User claim id to update</param>
    /// <param name="claim">Claim value to update</param>
    /// <param name="cancellationToken">Cancelation token</param>
    /// <returns>User claims</returns>
    [HttpPut("{id}/claim/{userClaimId}", Name = nameof(UpdateUserClaim))]
    public async Task<IActionResult> UpdateUserClaim(string id, string userClaimId, [FromBody] UpdateUserClaimModel claim, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(id))
        {
            return BadRequest($"{nameof(id)} must have a value");
        }

        if (string.IsNullOrEmpty(userClaimId))
        {
            return BadRequest($"{nameof(userClaimId)} must have a value");
        }

        if (claim is null || string.IsNullOrEmpty(claim.Value))
        {
            return BadRequest($"{nameof(claim)} must have a value");
        }

        Domain.Entities.UserId userId;
        if (Guid.TryParse(id, out var parsedUserId))
        {
            userId = new Domain.Entities.UserId(parsedUserId);
        }
        else
        {
            return BadRequest($"{nameof(id)} has a wrong format '{id}'");
        }

        Domain.Entities.UserClaimId validUserClaimId;
        if (Guid.TryParse(userClaimId, out var parsedUserClaimId))
        {
            validUserClaimId = new Domain.Entities.UserClaimId(parsedUserClaimId);
        }
        else
        {
            return BadRequest($"{nameof(userClaimId)} has a wrong format '{userClaimId}'");
        }

        var roles = await this.CommandForCurrentUser(cancellationToken).User.UpdateClaim(userId, validUserClaimId, claim);
        return Ok(roles);
    }

    /// <summary>
    /// Remove claims from user
    /// </summary>
    /// <param name="id">User identifier</param>
    /// <param name="userClaimId">User claim id to remove</param>
    /// <param name="cancellationToken">Cancelation token</param>
    /// <returns>User claims</returns>
    [HttpDelete("{id}/claim/{userClaimId}", Name = nameof(RemoveUserClaim))]
    public async Task<IActionResult> RemoveUserClaim(string id, string userClaimId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(id))
        {
            return BadRequest($"{nameof(id)} must have a value");
        }

        if (string.IsNullOrEmpty(userClaimId))
        {
            return BadRequest($"{nameof(userClaimId)} must have a value");
        }

        Domain.Entities.UserId userId;
        if (Guid.TryParse(id, out var parsedUserId))
        {
            userId = new Domain.Entities.UserId(parsedUserId);
        }
        else
        {
            return BadRequest($"{nameof(id)} has a wrong format '{id}'");
        }

        Domain.Entities.UserClaimId validUserClaimId;
        if (Guid.TryParse(userClaimId, out var parsedUserClaimId))
        {
            validUserClaimId = new Domain.Entities.UserClaimId(parsedUserClaimId);
        }
        else
        {
            return BadRequest($"{nameof(userClaimId)} has a wrong format '{userClaimId}'");
        }

        var roles = await this.CommandForCurrentUser(cancellationToken).User.RemoveClaim(userId, validUserClaimId);
        return Ok(roles);
    }
}
