namespace Animato.Sso.WebApi.Controllers;

using Animato.Sso.Application.Models;
using Animato.Sso.Domain.Entities;
using Animato.Sso.WebApi.Extensions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

public class AssetController : ApiControllerBase
{
    private readonly ILogger<AssetController> logger;

    public AssetController(ISender mediator, ILogger<AssetController> logger) : base(mediator) => this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>
    /// Create new asset
    /// </summary>
    /// <param name="file">Asset</param>
    /// <param name="transformations">List of transformations</param>
    /// <param name="tranformationId">Id of saved transformation definition</param>
    /// <param name="cancellationToken">Cancelation token</param>
    /// <returns>Asset  metadata</returns>
    [HttpPost(Name = "CreateAsset")]
    public async Task<ActionResult<AssetMetadata>> Create(IFormFile file, [FromQuery(Name = "t")] string[] transformations, [FromQuery(Name = "transformationId")] Guid? tranformationId, CancellationToken cancellationToken)
    {
        logger.LogDebug("Executing action {Action}", nameof(Create));
        var createAsset = new CreateAsset
        {
            Name = file.FileName,
            Stream = file.OpenReadStream(),
            ContentType = file.ContentType,
            TransformationId = tranformationId
        };

        if (transformations is not null)
        {
            createAsset.Transformations.AddRange(transformations);
        }

        return Ok(await this.Command(cancellationToken).Asset.Create(createAsset));
    }

    /// <summary>
    /// Update existing asset
    /// </summary>
    /// <param name="file">Asset</param>
    /// <param name="transformations">List of transformations</param>
    /// <param name="id">Asset identifier</param>
    /// <param name="cancellationToken">Cancelation token</param>
    /// <returns>Asset  metadata</returns>
    [HttpPut("{id:guid}", Name = "UpdateAsset")]
    public async Task<ActionResult<AssetMetadata>> Update(IFormFile file, [FromQuery(Name = "t")] string[] transformations, Guid id, CancellationToken cancellationToken)
    {
        logger.LogDebug("Executing action {Action}", nameof(Update));

        var updateAsset = new UpdateAsset
        {
            Id = id,
            Name = file.FileName,
            Stream = file.OpenReadStream(),
            ContentType = file.ContentType
        };

        if (transformations is not null)
        {
            updateAsset.Transformations.AddRange(transformations);
        }

        return Ok(await this.Command(cancellationToken).Asset.Update(updateAsset));
    }

    /// <summary>
    /// Delete asset
    /// </summary>
    /// <param name="id">Asset identifier</param>
    /// <param name="cancellationToken">Cancelation token</param>
    /// <returns></returns>
    [HttpDelete("{id:guid}", Name = "DeleteAsset")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        logger.LogDebug("Executing action {Action}", nameof(Delete));
        await this.Command(cancellationToken).Asset.Delete(id);
        return Ok();
    }

    /// <summary>
    /// Get asset metadata
    /// </summary>
    /// <param name="id">Asset identifier</param>
    /// <param name="cancellationToken">Cancelation token</param>
    /// <returns>Asset  metadata</returns>
    [HttpGet("{id:guid}", Name = "GetAssetById")]
    public async Task<ActionResult<AssetMetadata>> Get(Guid id, CancellationToken cancellationToken)
    {
        logger.LogDebug("Executing action {Action}", nameof(Get));
        var asset = await this.Query(cancellationToken).Asset.GetById(id);

        if (asset is null)
        {
            return NotFound();
        }

        return Ok(asset);
    }

    /// <summary>
    /// Get all asset metadata
    /// </summary>
    /// <param name="cancellationToken">Cancelation token</param>
    /// <returns>List of asset  metadata</returns>
    [HttpGet(Name = "GetAssetsAll")]
    public async Task<ActionResult<IEnumerable<AssetMetadata>>> GetAll(CancellationToken cancellationToken)
    {
        logger.LogDebug("Executing action {Action}", nameof(Get));
        var assets = await this.Query(cancellationToken).Asset.GetAll();

        if (assets is null || !assets.Any())
        {
            return NotFound();
        }

        return Ok(assets);
    }



}
