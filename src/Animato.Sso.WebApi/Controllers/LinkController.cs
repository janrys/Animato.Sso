namespace Animato.Sso.WebApi.Controllers;
using Animato.Sso.Domain.Entities;
using Animato.Sso.WebApi.Extensions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

public class LinkController : ApiControllerBase
{
    private readonly ILogger<LinkController> logger;

    public LinkController(ISender mediator, ILogger<LinkController> logger) : base(mediator) => this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>
    /// Get asset file by shortcut link
    /// </summary>
    /// <param name="id">Asset identifier</param>
    /// <param name="variantId">Asset variant identifier</param>
    /// <param name="cancellationToken">Cancelation token</param>
    /// <returns>Asset  metadata</returns>
    [HttpGet("{id:guid}/{variantId:guid}", Name = "GetAssetFileByShortcut")]
    public async Task<ActionResult<AssetMetadata>> Get(Guid id, Guid variantId, CancellationToken cancellationToken)
    {
        logger.LogDebug("Executing action {Action}", nameof(Get));
        var asset = await this.Query(cancellationToken).Asset.GetById(id);

        if (asset is null)
        {
            return NotFound();
        }

        var variant = asset.Variants.FirstOrDefault(v => v.Id.Equals(variantId));

        if (variant is null || string.IsNullOrEmpty(variant.Url))
        {
            return NotFound();
        }

        return Redirect(variant.Url);
    }
}
