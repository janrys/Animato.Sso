namespace Animato.Sso.Application.Features.Assets;
using System;
using System.Threading;
using System.Threading.Tasks;
using Animato.Sso.Application.Common.Interfaces;
using Animato.Sso.Application.Exceptions;
using Animato.Sso.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

public class GetAssetQuery : IRequest<AssetMetadata>
{
    public GetAssetQuery(Guid id) => Id = id;

    public Guid Id { get; }

    public class GetPartnerQueryHandler : IRequestHandler<GetAssetQuery, AssetMetadata>
    {
        private readonly IMetadataStorageService storage;
        private readonly ILogger<GetPartnerQueryHandler> logger;
        private const string ERROR_LOADING_ASSETS = "Error loading assets";

        public GetPartnerQueryHandler(IMetadataStorageService storage, ILogger<GetPartnerQueryHandler> logger)
        {
            this.storage = storage ?? throw new ArgumentNullException(nameof(storage));
            this.logger = logger;
        }

        public Task<AssetMetadata> Handle(GetAssetQuery request, CancellationToken cancellationToken)
        {
            try
            {
                return storage.GetAsset(request.Id, cancellationToken);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, ERROR_LOADING_ASSETS);
                throw new DataAccessException(ERROR_LOADING_ASSETS, exception);
            }
        }
    }
}
