namespace Animato.Sso.WebApi.Controllers;
using Animato.Sso.Application.Features.Users.DTOs;
using Animato.Sso.WebApi.Extensions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

public class UserController : ApiControllerBase
{
    private readonly ILogger<UserController> logger;

    public UserController(ISender mediator, ILogger<UserController> logger) : base(mediator) => this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>
    /// Get all users
    /// </summary>
    /// <param name="login">User login</param>
    /// <param name="cancellationToken">Cancelation token</param>
    /// <returns>List of users</returns>
    [HttpGet(Name = "GetUsers")]
    public async Task<IActionResult> GetAll([FromQuery] string login, CancellationToken cancellationToken)
    {
        logger.LogDebug("Executing action {Action}", nameof(GetAll));

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
    [HttpGet("{id}", Name = "GetUserById")]
    public async Task<IActionResult> GetById(string id, CancellationToken cancellationToken)
    {
        logger.LogDebug("Executing action {Action}", nameof(GetById));

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
        logger.LogDebug("Executing action {Action}", nameof(login));

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
    [HttpPost(Name = "CreateUser")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserModel user, CancellationToken cancellationToken)
    {
        logger.LogDebug("Executing action {Action}", nameof(CreateUser));

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
    [HttpPut("{id}", Name = "UpdateUser")]
    public async Task<IActionResult> UpdateUser(string id, [FromBody] CreateUserModel user, CancellationToken cancellationToken)
    {
        logger.LogDebug("Executing action {Action}", nameof(UpdateUser));

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
    [HttpDelete("{id}", Name = "DeleteUser")]
    public async Task<IActionResult> DeleteUser(string id, CancellationToken cancellationToken)
    {
        logger.LogDebug("Executing action {Action}", nameof(DeleteUser));

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
    /// Assign application role to user
    /// </summary>
    /// <param name="id">User identifier</param>
    /// <param name="roleId">Application user role identifier</param>
    /// <param name="cancellationToken">Cancelation token</param>
    /// <returns>User application roles</returns>
    [HttpPost("{id}/role/{roleId}", Name = "AssignUserRole")]
    public async Task<IActionResult> AssignUserRole(string id, string roleId, CancellationToken cancellationToken)
    {
        logger.LogDebug("Executing action {Action}", nameof(AssignUserRole));

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
        logger.LogDebug("Executing action {Action}", nameof(GetUserRoles));

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
        logger.LogDebug("Executing action {Action}", nameof(UnassignUserRole));

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
}
