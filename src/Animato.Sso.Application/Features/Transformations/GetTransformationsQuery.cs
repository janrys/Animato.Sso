namespace Animato.Sso.Application.Features.Transformations;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Animato.Sso.Application.Common.Interfaces;
using Animato.Sso.Application.Exceptions;
using Animato.Sso.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

public class GetTransformationsQuery : IRequest<IEnumerable<TransformationDefinition>>
{
    public class GetTransformationsQueryHandler : IRequestHandler<GetTransformationsQuery, IEnumerable<TransformationDefinition>>
    {
        private readonly ITransformationStorageService storage;
        private readonly ILogger<GetTransformationsQueryHandler> logger;
        private const string ERROR_LOADING_TRANSFORMATIONS = "Error loading transformations";

        public GetTransformationsQueryHandler(ITransformationStorageService storage, ILogger<GetTransformationsQueryHandler> logger)
        {
            this.storage = storage ?? throw new ArgumentNullException(nameof(storage));
            this.logger = logger;
        }

        public Task<IEnumerable<TransformationDefinition>> Handle(GetTransformationsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                return storage.GetTransformations(cancellationToken);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, ERROR_LOADING_TRANSFORMATIONS);
                throw new DataAccessException(ERROR_LOADING_TRANSFORMATIONS, exception);
            }
        }
    }
}
