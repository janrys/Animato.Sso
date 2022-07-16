namespace Animato.Sso.WebApi.Controllers;
using Animato.Sso.Application.Features.Applications.DTOs;
using Animato.Sso.Application.Features.Scopes.DTOs;
using Animato.Sso.WebApi.Extensions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status500InternalServerError)]
public class ApplicationController : ApiControllerBase
{
    public ApplicationController(ISender mediator) : base(mediator) { }

    /// <summary>
    /// Get all applications
    /// </summary>
    /// <param name="clientId"></param>
    /// <param name="cancellationToken">Cancelation token</param>
    /// <returns>List of applications</returns>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Domain.Entities.Application>))]
    [HttpGet(Name = "GetApplications")]
    public async Task<IActionResult> GetAll([FromQuery] string clientId, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(clientId))
        {
            return await GetByCode(clientId, cancellationToken);
        }

        var applications = await this.QueryForCurrentUser(cancellationToken).Application.GetAll();
        return Ok(applications);
    }

    /// <summary>
    /// Get application
    /// </summary>
    /// <param name="id">Application id</param>
    /// <param name="cancellationToken">Cancelation token</param>
    /// <returns>Application</returns>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Domain.Entities.Application))]
    [HttpGet("{id}", Name = "GetApplicationById")]
    public async Task<IActionResult> GetById(string id, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(id))
        {
            return BadRequest($"{nameof(id)} must have a value");
        }
        Domain.Entities.ApplicationId applicationId;
        if (Guid.TryParse(id, out var parsedApplicationId))
        {
            applicationId = new Domain.Entities.ApplicationId(parsedApplicationId);
        }
        else
        {
            return BadRequest($"{nameof(id)} has a wrong format '{id}'");
        }

        var application = await this.QueryForCurrentUser(cancellationToken).Application.GetById(applicationId);

        if (application is null)
        {
            return NotFound();
        }

        return Ok(application);
    }

    private async Task<IActionResult> GetByCode([FromQuery] string clientId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(clientId))
        {
            return BadRequest($"{nameof(clientId)} must have a value");
        }

        var application = await this.QueryForCurrentUser(cancellationToken).Application.GetByClientId(clientId);

        if (application is null)
        {
            return NotFound();
        }

        return Ok(new List<Domain.Entities.Application>() { application });
    }

    /// <summary>
    /// Create application
    /// </summary>
    /// <param name="application"></param>
    /// <param name="cancellationToken">Cancelation token</param>
    /// <returns>Created application</returns>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Domain.Entities.Application))]
    [HttpPost(Name = "CreateApplication")]
    public async Task<IActionResult> CreateApplication([FromBody] CreateApplicationModel application, CancellationToken cancellationToken)
    {
        if (application is null)
        {
            return BadRequest($"{nameof(application)} must have a value");
        }

        var createdApplication = await this.CommandForCurrentUser(cancellationToken).Application.Create(application);
        return Ok(createdApplication);
    }

    /// <summary>
    /// Update application
    /// </summary>
    /// <param name="id">Application id to update</param>
    /// <param name="application">Application changes</param>
    /// <param name="cancellationToken">Cancelation token</param>
    /// <returns>Updated application</returns>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Domain.Entities.Application))]
    [HttpPut("{id}", Name = "UpdateApplication")]
    public async Task<IActionResult> UpdateApplication(string id, [FromBody] CreateApplicationModel application, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(id))
        {
            return BadRequest($"{nameof(id)} must have a value");
        }

        Domain.Entities.ApplicationId applicationId;
        if (Guid.TryParse(id, out var parsedApplicationId))
        {
            applicationId = new Domain.Entities.ApplicationId(parsedApplicationId);
        }
        else
        {
            return BadRequest($"{nameof(id)} has a wrong format '{id}'");
        }

        if (application is null)
        {
            return BadRequest($"{nameof(application)} must have a value");
        }

        var createdApplication = await this.CommandForCurrentUser(cancellationToken).Application.Update(applicationId, application);
        return Ok(createdApplication);
    }

    /// <summary>
    /// Delete application
    /// </summary>
    /// <param name="id">Application id to delete</param>
    /// <param name="cancellationToken">Cancelation token</param>
    /// <returns></returns>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [HttpDelete("{id}", Name = "DeleteApplication")]
    public async Task<IActionResult> DeleteApplication(string id, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(id))
        {
            return BadRequest($"{nameof(id)} must have a value");
        }

        Domain.Entities.ApplicationId applicationId;
        if (Guid.TryParse(id, out var parsedApplicationId))
        {
            applicationId = new Domain.Entities.ApplicationId(parsedApplicationId);
        }
        else
        {
            return BadRequest($"{nameof(id)} has a wrong format '{id}'");
        }

        await this.CommandForCurrentUser(cancellationToken).Application.Delete(applicationId);
        return Ok();
    }

    /// <summary>
    /// Get all roles of the application
    /// </summary>
    /// <param name="id">Application identifier</param>
    /// <param name="cancellationToken">Cancelation token</param>
    /// <returns>List of application roles</returns>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Domain.Entities.ApplicationRole>))]
    [HttpGet("{id}/role", Name = "GetAllApplicationRoles")]
    public async Task<IActionResult> GetAllApplicationRoles(string id, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(id))
        {
            return BadRequest($"{nameof(id)} must have a value");
        }
        Domain.Entities.ApplicationId applicationId;
        if (Guid.TryParse(id, out var parsedApplicationId))
        {
            applicationId = new Domain.Entities.ApplicationId(parsedApplicationId);
        }
        else
        {
            return BadRequest($"{nameof(id)} has a wrong format '{id}'");
        }

        var roles = await this.QueryForCurrentUser(cancellationToken).Application.GetRolesByApplicationId(applicationId);

        return Ok(roles);
    }

    /// <summary>
    /// Get application role
    /// </summary>
    /// <param name="id">Application role id</param>
    /// <param name="cancellationToken">Cancelation token</param>
    /// <returns>Application role</returns>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Domain.Entities.ApplicationRole))]
    [HttpGet("role/{id}", Name = "GetRoleById")]
    public async Task<IActionResult> GetRoleById(string id, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(id))
        {
            return BadRequest($"{nameof(id)} must have a value");
        }
        Domain.Entities.ApplicationRoleId applicationRoleId;
        if (Guid.TryParse(id, out var parsedApplicationRoleId))
        {
            applicationRoleId = new Domain.Entities.ApplicationRoleId(parsedApplicationRoleId);
        }
        else
        {
            return BadRequest($"{nameof(id)} has a wrong format '{id}'");
        }

        var role = await this.QueryForCurrentUser(cancellationToken).Application.GetRoleById(applicationRoleId);

        if (role is null)
        {
            return NotFound();
        }

        return Ok(role);
    }

    /// <summary>
    /// Create application roles
    /// </summary>
    /// <param name="id">Application identifier</param>
    /// <param name="roles">Application roles</param>
    /// <param name="cancellationToken">Cancelation token</param>
    /// <returns>Created application roles</returns>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Domain.Entities.ApplicationRole>))]
    [HttpPost("{id}/role", Name = "CreateApplicationRoles")]
    public async Task<IActionResult> CreateApplicationRoles(string id, [FromBody] CreateApplicationRolesModel roles, CancellationToken cancellationToken)
    {
        if (roles is null)
        {
            return BadRequest($"{nameof(roles)} must have a value");
        }

        if (string.IsNullOrEmpty(id))
        {
            return BadRequest($"{nameof(id)} must have a value");
        }

        if (roles.Names is null || !roles.Names.Any() || roles.Names.Any(n => string.IsNullOrEmpty(n.Trim())))
        {
            return BadRequest($"{nameof(roles.Names)} must have a value");
        }

        Domain.Entities.ApplicationId applicationId;
        if (Guid.TryParse(id, out var parsedApplicationId))
        {
            applicationId = new Domain.Entities.ApplicationId(parsedApplicationId);
        }
        else
        {
            return BadRequest($"{nameof(id)} has a wrong format '{id}'");
        }

        var createdRoles = await this.CommandForCurrentUser(cancellationToken).Application.CreateRole(applicationId, roles);
        return Ok(createdRoles);
    }

    /// <summary>
    /// Update application role
    /// </summary>
    /// <param name="id">Application role id to update</param>
    /// <param name="role">Application role changes</param>
    /// <param name="cancellationToken">Cancelation token</param>
    /// <returns>Updated application role</returns>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Domain.Entities.ApplicationRole))]
    [HttpPut("role/{id}", Name = "UpdateApplicationRole")]
    public async Task<IActionResult> UpdateApplicationRole(string id, [FromBody] CreateApplicationRoleModel role, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(id))
        {
            return BadRequest($"{nameof(id)} must have a value");
        }

        Domain.Entities.ApplicationRoleId roleId;
        if (Guid.TryParse(id, out var parsedApplicationRoleId))
        {
            roleId = new Domain.Entities.ApplicationRoleId(parsedApplicationRoleId);
        }
        else
        {
            return BadRequest($"{nameof(id)} has a wrong format '{id}'");
        }

        if (role is null)
        {
            return BadRequest($"{nameof(role)} must have a value");
        }

        var createdApplicationRole = await this.CommandForCurrentUser(cancellationToken).Application.UpdateRole(roleId, role);
        return Ok(createdApplicationRole);
    }

    /// <summary>
    /// Delete application role
    /// </summary>
    /// <param name="id">Application role id to delete</param>
    /// <param name="cancellationToken">Cancelation token</param>
    /// <returns></returns>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [HttpDelete("role/{id}", Name = "DeleteApplicationRole")]
    public async Task<IActionResult> DeleteApplicationRole(string id, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(id))
        {
            return BadRequest($"{nameof(id)} must have a value");
        }

        Domain.Entities.ApplicationRoleId roleId;
        if (Guid.TryParse(id, out var parsedApplicationRoleId))
        {
            roleId = new Domain.Entities.ApplicationRoleId(parsedApplicationRoleId);
        }
        else
        {
            return BadRequest($"{nameof(id)} has a wrong format '{id}'");
        }

        await this.CommandForCurrentUser(cancellationToken).Application.DeleteRole(roleId);
        return Ok();
    }

    /// <summary>
    /// Get all scopes of the application
    /// </summary>
    /// <param name="id">Application identifier</param>
    /// <param name="cancellationToken">Cancelation token</param>
    /// <returns>List of scopes allowed for the application</returns>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Domain.Entities.Scope>))]
    [HttpGet("{id}/scope", Name = "GetAllApplicationScopes")]
    public async Task<IActionResult> GetAllApplicationScopes(string id, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(id))
        {
            return BadRequest($"{nameof(id)} must have a value");
        }
        Domain.Entities.ApplicationId applicationId;
        if (Guid.TryParse(id, out var parsedApplicationId))
        {
            applicationId = new Domain.Entities.ApplicationId(parsedApplicationId);
        }
        else
        {
            return BadRequest($"{nameof(id)} has a wrong format '{id}'");
        }

        var scopes = await this.QueryForCurrentUser(cancellationToken).Application.GetScopes(applicationId);

        return Ok(scopes);
    }

    /// <summary>
    /// Add allowed scopes to application
    /// </summary>
    /// <param name="id">Application identifier</param>
    /// <param name="scopes">List of scope names</param>
    /// <param name="cancellationToken">Cancelation token</param>
    /// <returns>All scopes allowed for application</returns>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Domain.Entities.Scope>))]
    [HttpPost("{id}/scope", Name = "AddApplicationScopes")]
    public async Task<IActionResult> AddApplicationScopes(string id, [FromBody] CreateScopesModel scopes, CancellationToken cancellationToken)
    {
        if (scopes is null || scopes.Names is null
            || !scopes.Names.Any()
            || !scopes.Names.Any(n => !string.IsNullOrEmpty(n)))
        {
            return BadRequest($"{nameof(scopes)} must have a value");
        }

        if (string.IsNullOrEmpty(id))
        {
            return BadRequest($"{nameof(id)} must have a value");
        }

        Domain.Entities.ApplicationId applicationId;
        if (Guid.TryParse(id, out var parsedApplicationId))
        {
            applicationId = new Domain.Entities.ApplicationId(parsedApplicationId);
        }
        else
        {
            return BadRequest($"{nameof(id)} has a wrong format '{id}'");
        }

        await this.CommandForCurrentUser(cancellationToken).Application.AddScopes(applicationId, scopes);
        var allScopes = await this.QueryForCurrentUser(cancellationToken).Application.GetScopes(applicationId);
        return Ok(allScopes);
    }

    /// <summary>
    /// Delete application role
    /// </summary>
    /// <param name="id">Application id to delete</param>
    /// <param name="scopes">List of scopes to remove from application</param>
    /// <param name="cancellationToken">Cancelation token</param>
    /// <returns>All scopes allowed for application</returns>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Domain.Entities.Scope>))]
    [HttpDelete("{id}/scope", Name = "RemoveApplicationScopes")]
    public async Task<IActionResult> RemoveApplicationScopes(string id, [FromBody] CreateScopesModel scopes, CancellationToken cancellationToken)
    {
        if (scopes is null || scopes.Names is null
            || !scopes.Names.Any()
            || !scopes.Names.Any(n => !string.IsNullOrEmpty(n)))
        {
            return BadRequest($"{nameof(scopes)} must have a value");
        }

        if (string.IsNullOrEmpty(id))
        {
            return BadRequest($"{nameof(id)} must have a value");
        }

        Domain.Entities.ApplicationId applicationId;
        if (Guid.TryParse(id, out var parsedApplicationId))
        {
            applicationId = new Domain.Entities.ApplicationId(parsedApplicationId);
        }
        else
        {
            return BadRequest($"{nameof(id)} has a wrong format '{id}'");
        }

        await this.CommandForCurrentUser(cancellationToken).Application.RemoveScopes(applicationId, scopes);
        var allScopes = await this.QueryForCurrentUser(cancellationToken).Application.GetScopes(applicationId);
        return Ok(allScopes);
    }
}
