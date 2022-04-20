namespace Animato.Sso.Application.Features.Assets;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Animato.Sso.Application.Common.Interfaces;
using Animato.Sso.Application.Exceptions;
using Animato.Sso.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

public class GetAssetsQuery : IRequest<IEnumerable<AssetMetadata>>
{
    public class GetAssetsQueryHandler : IRequestHandler<GetAssetsQuery, IEnumerable<AssetMetadata>>
    {
        private readonly IMetadataStorageService storage;
        private readonly ILogger<GetAssetsQueryHandler> logger;
        private const string ERROR_LOADING_ASSETS = "Error loading assets";

        public GetAssetsQueryHandler(IMetadataStorageService storage, ILogger<GetAssetsQueryHandler> logger)
        {
            this.storage = storage ?? throw new ArgumentNullException(nameof(storage));
            this.logger = logger;
        }

        public Task<IEnumerable<AssetMetadata>> Handle(GetAssetsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                return storage.GetAssets(cancellationToken);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, ERROR_LOADING_ASSETS);
                throw new DataAccessException(ERROR_LOADING_ASSETS, exception);
            }
        }
    }
}
