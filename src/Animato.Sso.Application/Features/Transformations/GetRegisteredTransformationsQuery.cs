namespace Animato.Sso.Application.Features.Assets;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Animato.Sso.Application.Common.Interfaces;
using Animato.Sso.Application.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

public class GetRegisteredTransformationsQuery : IRequest<IEnumerable<string>>
{
    public class GetRegisteredTransformationsQueryHandler : IRequestHandler<GetRegisteredTransformationsQuery, IEnumerable<string>>
    {
        private readonly IAssetTransformationFactory assetTransformationFactory;
        private readonly ILogger<GetRegisteredTransformationsQueryHandler> logger;
        private const string ERROR_LOADING_TRANSFORMATIONS = "Error loading transformations";

        public GetRegisteredTransformationsQueryHandler(IAssetTransformationFactory assetTransformationFactory, ILogger<GetRegisteredTransformationsQueryHandler> logger)
        {
            this.assetTransformationFactory = assetTransformationFactory ?? throw new ArgumentNullException(nameof(assetTransformationFactory));
            this.logger = logger;
        }

        public Task<IEnumerable<string>> Handle(GetRegisteredTransformationsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                return Task.FromResult(assetTransformationFactory.GetTransformations().OrderBy(t => t.Code).Select(t => $"{t.Code}: types ({string.Join(", ", t.AssetTypes)}). {t.Description}"));
            }
            catch (Exception exception)
            {
                logger.LogError(exception, ERROR_LOADING_TRANSFORMATIONS);
                throw new DataAccessException(ERROR_LOADING_TRANSFORMATIONS, exception);
            }
        }
    }
}
