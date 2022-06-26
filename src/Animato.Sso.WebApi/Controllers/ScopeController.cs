namespace Animato.Sso.WebApi.Controllers;
using Animato.Sso.Application.Features.Scopes.DTOs;
using Animato.Sso.Domain.Entities;
using Animato.Sso.WebApi.Extensions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

public class ScopeController : ApiControllerBase
{
    private readonly ILogger<ScopeController> logger;

    public ScopeController(ISender mediator, ILogger<ScopeController> logger) : base(mediator) => this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>
    /// Get all scopes
    /// </summary>
    /// <param name="name">Scope name</param>
    /// <param name="cancellationToken">Cancelation token</param>
    /// <returns>List of scopes</returns>
    [HttpGet(Name = "GetScopes")]
    public async Task<IActionResult> GetAll([FromQuery] string name, CancellationToken cancellationToken)
    {
        logger.LogDebug("Executing action {Action}", nameof(GetAll));

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
        logger.LogDebug("Executing action {Action}", nameof(CreateScopes));

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
        logger.LogDebug("Executing action {Action}", nameof(UpdateScope));

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
        logger.LogDebug("Executing action {Action}", nameof(DeleteScope));

        if (string.IsNullOrEmpty(name))
        {
            return BadRequest($"{nameof(name)} must have a value");
        }

        await this.CommandForCurrentUser(cancellationToken).Scope.Delete(name);
        return Ok();
    }

}
