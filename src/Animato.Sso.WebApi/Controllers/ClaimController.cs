namespace Animato.Sso.WebApi.Controllers;

using Animato.Sso.Application.Features.Claims.DTOs;
using Animato.Sso.Domain.Entities;
using Animato.Sso.WebApi.Extensions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

public class ClaimController : ApiControllerBase
{
    private readonly ILogger<ClaimController> logger;

    public ClaimController(ISender mediator, ILogger<ClaimController> logger) : base(mediator) => this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>
    /// Get all claims
    /// </summary>
    /// <param name="name">Claim name</param>
    /// <param name="cancellationToken">Cancelation token</param>
    /// <returns>List of claims</returns>
    [HttpGet(Name = "GetClaims")]
    public async Task<IActionResult> GetAll([FromQuery] string name, CancellationToken cancellationToken)
    {
        logger.LogDebug("Executing action {Action}", nameof(GetAll));

        var claims = new List<Claim>();
        if (!string.IsNullOrEmpty(name))
        {
            var claim = await this.QueryForCurrentUser(cancellationToken).Claim.GetByName(name);
            claims.Add(claim);
        }

        claims.AddRange(await this.QueryForCurrentUser(cancellationToken).Claim.GetAll());
        return Ok(claims);
    }


    /// <summary>
    /// Create claim
    /// </summary>
    /// <param name="claim"></param>
    /// <param name="cancellationToken">Cancelation token</param>
    /// <returns>Created claim</returns>
    [HttpPost(Name = "CreateClaim")]
    public async Task<IActionResult> CreateClaim([FromBody] CreateClaimModel claim, CancellationToken cancellationToken)
    {
        logger.LogDebug("Executing action {Action}", nameof(CreateClaim));

        if (claim is null || string.IsNullOrEmpty(claim.Name))
        {
            return BadRequest($"{nameof(claim)} and {nameof(claim.Name)} must have a value");
        }

        var createdClaim = await this.CommandForCurrentUser(cancellationToken).Claim.Create(claim);
        return Ok(createdClaim);
    }

    /// <summary>
    /// Update scope
    /// </summary>
    /// <param name="name">Current claim name</param>
    /// <param name="claim">Claim data to update</param>
    /// <param name="cancellationToken">Cancelation token</param>
    /// <returns>Updated claim</returns>
    [HttpPut("{name}", Name = "UpdateClaim")]
    public async Task<IActionResult> UpdateClaim(string name, [FromBody] CreateClaimModel claim, CancellationToken cancellationToken)
    {
        logger.LogDebug("Executing action {Action}", nameof(UpdateClaim));

        if (string.IsNullOrEmpty(name))
        {
            return BadRequest($"{nameof(name)} must have a value");
        }

        if (claim is null || string.IsNullOrEmpty(claim.Name))
        {
            return BadRequest($"{nameof(claim)} and {nameof(claim.Name)} must have a value");
        }

        var updatedClaim = await this.CommandForCurrentUser(cancellationToken).Claim.Update(name, claim);
        return Ok(updatedClaim);
    }

    /// <summary>
    /// Delete claim
    /// </summary>
    /// <param name="name">Claim name to delete</param>
    /// <param name="cancellationToken">Cancelation token</param>
    /// <returns></returns>
    [HttpDelete("{name}", Name = "DeleteClaim")]
    public async Task<IActionResult> DeleteClaim(string name, CancellationToken cancellationToken)
    {
        logger.LogDebug("Executing action {Action}", nameof(DeleteClaim));

        if (string.IsNullOrEmpty(name))
        {
            return BadRequest($"{nameof(name)} must have a value");
        }

        await this.CommandForCurrentUser(cancellationToken).Claim.Delete(name);
        return Ok();
    }

}
