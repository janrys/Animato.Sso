namespace Animato.Sso.WebApi.Controllers;

using Animato.Sso.Application.Common.Interfaces;
using Animato.Sso.Application.Models;
using Animato.Sso.Domain.Entities;
using Animato.Sso.WebApi.Extensions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

public class TransformController : ApiControllerBase
{
    private readonly ILogger<TransformController> logger;

    public TransformController(ISender mediator, ILogger<TransformController> logger) : base(mediator) => this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>
    /// Get all registered transformation codes for asset types
    /// </summary>
    /// <param name="assetTransformationFactory">Factory to get registered transformations</param>
    /// <param name="cancellationToken">Cancelation token</param>
    /// <returns>List of asset transformations codes and their asset types</returns>
    [HttpGet("registered", Name = "GetRegisteredTransformations")]
    public async Task<ActionResult<IEnumerable<string>>> GetRegisteredTransformations([FromServices] IAssetTransformationFactory assetTransformationFactory, CancellationToken cancellationToken)
    {
        logger.LogDebug("Executing action {Action}", nameof(GetRegisteredTransformations));
        var transformations = await this.Query(cancellationToken).Transformation.GetRegistered();
        return Ok(transformations);
    }

    /// <summary>
    /// Create transformation
    /// </summary>
    /// <param name="transformation">List of transformations with parameters</param>
    /// <param name="cancellationToken">Cancelation token</param>
    /// <returns>Transformation definition</returns>
    [HttpPost(Name = "CreateTransformation")]
    public async Task<ActionResult<TransformationDefinition>> Create([FromBody] CreateTransformationDefinition transformation, CancellationToken cancellationToken)
    {
        logger.LogDebug("Executing action {Action}", nameof(Create));
        if (transformation is null)
        {
            return BadRequest();
        }

        return Ok(await this.Command(cancellationToken).Transformation.Create(transformation));
    }

    /// <summary>
    /// Update existing transformation
    /// </summary>
    /// <param name="transformation">List of transformations</param>
    /// <param name="id">TRansformation identifier</param>
    /// <param name="cancellationToken">Cancelation token</param>
    /// <returns>Transformation definition</returns>
    [HttpPut("{id:guid}", Name = "UpdateTransformation")]
    public async Task<ActionResult<TransformationDefinition>> Update([FromBody] CreateTransformationDefinition transformation, Guid id, CancellationToken cancellationToken)
    {
        logger.LogDebug("Executing action {Action}", nameof(Update));

        if (transformation is null)
        {
            return BadRequest();
        }

        var updateTransformation = new UpdateTransformationDefinition
        {
            Id = id,
            Definition = transformation.Definition,
            Description = transformation.Description
        };

        return Ok(await this.Command(cancellationToken).Transformation.Update(updateTransformation));
    }

    /// <summary>
    /// Delete asset
    /// </summary>
    /// <param name="id">Asset identifier</param>
    /// <param name="cancellationToken">Cancelation token</param>
    /// <returns></returns>
    [HttpDelete("{id:guid}", Name = "Transformation")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        logger.LogDebug("Executing action {Action}", nameof(Delete));
        await this.Command(cancellationToken).Transformation.Delete(id);
        return Ok();
    }

    /// <summary>
    /// Get transformation definition
    /// </summary>
    /// <param name="id">Asset identifier</param>
    /// <param name="cancellationToken">Cancelation token</param>
    /// <returns>Transformation definition</returns>
    [HttpGet("{id:guid}", Name = "GetTransformationById")]
    public async Task<ActionResult<TransformationDefinition>> Get(Guid id, CancellationToken cancellationToken)
    {
        logger.LogDebug("Executing action {Action}", nameof(Get));
        var asset = await this.Query(cancellationToken).Transformation.GetById(id);

        if (asset is null)
        {
            return NotFound();
        }

        return Ok(asset);
    }

    /// <summary>
    /// Get all transformation definitions
    /// </summary>
    /// <param name="cancellationToken">Cancelation token</param>
    /// <returns>List of saved transformations</returns>
    [HttpGet(Name = "GetTransformationsAll")]
    public async Task<ActionResult<IEnumerable<TransformationDefinition>>> GetAll(CancellationToken cancellationToken)
    {
        logger.LogDebug("Executing action {Action}", nameof(Get));
        var assets = await this.Query(cancellationToken).Transformation.GetAll();

        if (assets is null || !assets.Any())
        {
            return NotFound();
        }

        return Ok(assets);
    }
}
