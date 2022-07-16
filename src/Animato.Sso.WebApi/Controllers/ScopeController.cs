namespace Animato.Sso.WebApi.Controllers;
using Animato.Sso.Application.Features.Scopes.DTOs;
using Animato.Sso.Domain.Entities;
using Animato.Sso.WebApi.Extensions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

[ProducesResponseType(StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status500InternalServerError)]

public class ScopeController : ApiControllerBase
{
    public ScopeController(ISender mediator) : base(mediator) { }

    /// <summary>
    /// Get all scopes
    /// </summary>
    /// <param name="name">Scope name</param>
    /// <param name="cancellationToken">Cancelation token</param>
    /// <returns>List of scopes</returns>
    [HttpGet(Name = "GetScopes")]
    public async Task<IActionResult> GetAll([FromQuery] string name, CancellationToken cancellationToken)
    {
        var scopes = new List<Scope>();
        if (!string.IsNullOrEmpty(name))
        {
            var scope = await this.QueryForCurrentUser(cancellationToken).Scope.GetByName(name);
            scopes.Add(scope);
        }

        scopes.AddRange(await this.QueryForCurrentUser(cancellationToken).Scope.GetAll());
        return Ok(scopes);
    }


    /// <summary>
    /// Create scope
    /// </summary>
    /// <param name="scopes"></param>
    /// <param name="cancellationToken">Cancelation token</param>
    /// <returns>Created scope</returns>
    [HttpPost(Name = "CreateScopes")]
    public async Task<IActionResult> CreateScopes([FromBody] CreateScopesModel scopes, CancellationToken cancellationToken)
    {
        if (scopes is null || scopes.Names is null
            || !scopes.Names.Any()
            || !scopes.Names.Any(n => !string.IsNullOrEmpty(n)))
        {
            return BadRequest($"{nameof(scopes)} must have a value");
        }

        var createdScope = await this.CommandForCurrentUser(cancellationToken).Scope.Create(scopes);
        return Ok(createdScope);
    }

    /// <summary>
    /// Update scope
    /// </summary>
    /// <param name="name">Current scope name</param>
    /// <param name="newName">New scope name</param>
    /// <param name="cancellationToken">Cancelation token</param>
    /// <returns>Updated scope</returns>
    [HttpPut("{name}/{new-name}", Name = "UpdateScope")]
    public async Task<IActionResult> UpdateScope(string name, [FromRoute(Name = "new-name")] string newName, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(name))
        {
            return BadRequest($"{nameof(name)} must have a value");
        }

        if (string.IsNullOrEmpty(newName))
        {
            return BadRequest($"{nameof(newName)} must have a value");
        }

        var updatedScope = await this.CommandForCurrentUser(cancellationToken).Scope.Update(name, newName);
        return Ok(updatedScope);
    }

    /// <summary>
    /// Delete scope
    /// </summary>
    /// <param name="name">Scope name to delete</param>
    /// <param name="cancellationToken">Cancelation token</param>
    /// <returns></returns>
    [HttpDelete("{name}", Name = "DeleteScope")]
    public async Task<IActionResult> DeleteScope(string name, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(name))
        {
            return BadRequest($"{nameof(name)} must have a value");
        }

        await this.CommandForCurrentUser(cancellationToken).Scope.Delete(name);
        return Ok();
    }

    /// <summary>
    /// Get claims assigned to scope
    /// </summary>
    /// <param name="name">Scope name</param>
    /// <param name="cancellationToken">Cancelation token</param>
    /// <returns>List of claims</returns>
    [HttpGet("{name}/scope", Name = "GetScopeClaims")]
    public async Task<IActionResult> GetScopeClaims(string name, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(name))
        {
            return BadRequest($"{nameof(name)} must have a value");
        }

        return Ok(await this.QueryForCurrentUser(cancellationToken).Scope.GetClaims(name));
    }

    /// <summary>
    /// Assign scope to claim
    /// </summary>
    /// <param name="name">Scope name</param>
    /// <param name="claimName">Claim name</param>
    /// <param name="cancellationToken">Cancelation token</param>
    /// <returns>List of scope claims</returns>
    [HttpPost("{name}/scope/{claim-name}", Name = nameof(AddScopeClaim))]
    public async Task<IActionResult> AddScopeClaim(string name, [FromRoute(Name = "claim-name")] string claimName, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(name))
        {
            return BadRequest($"{nameof(name)} must have a value");
        }

        if (string.IsNullOrEmpty(claimName))
        {
            return BadRequest($"{nameof(claimName)} must have a value");
        }

        var claims = await this.CommandForCurrentUser(cancellationToken).Scope.AddClaim(name, claimName);
        return Ok(claims);
    }

    /// <summary>
    /// Unassign scope from claim
    /// </summary>
    /// <param name="name">Scope name</param>
    /// <param name="claimName">Claim name</param>
    /// <param name="cancellationToken">Cancelation token</param>
    /// <returns>List of scope claims</returns>
    [HttpDelete("{name}/scope/{claim-name}", Name = nameof(DeleteScopeClaim))]
    public async Task<IActionResult> DeleteScopeClaim(string name, [FromRoute(Name = "claim-name")] string claimName, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(name))
        {
            return BadRequest($"{nameof(name)} must have a value");
        }

        var claims = await this.CommandForCurrentUser(cancellationToken).Scope.RemoveClaim(name, claimName);
        return Ok(claims);
    }

}
