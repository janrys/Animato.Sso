namespace Animato.Sso.Application.Features.Assets;
using System;
using System.Threading;
using System.Threading.Tasks;
using Animato.Sso.Application.Common.Interfaces;
using Animato.Sso.Application.Exceptions;
using Animato.Sso.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

public class GetTransformationQuery : IRequest<TransformationDefinition>
{
    public GetTransformationQuery(Guid id) => Id = id;

    public Guid Id { get; }

    public class GetTransformationQueryHandler : IRequestHandler<GetTransformationQuery, TransformationDefinition>
    {
        private readonly ITransformationStorageService storage;
        private readonly ILogger<GetTransformationQueryHandler> logger;
        private const string ERROR_LOADING_TRANSFORMATION = "Error loading transformation";

        public GetTransformationQueryHandler(ITransformationStorageService storage, ILogger<GetTransformationQueryHandler> logger)
        {
            this.storage = storage ?? throw new ArgumentNullException(nameof(storage));
            this.logger = logger;
        }

        public Task<TransformationDefinition> Handle(GetTransformationQuery request, CancellationToken cancellationToken)
        {
            try
            {
                return storage.GetTransformation(request.Id, cancellationToken);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, ERROR_LOADING_TRANSFORMATION);
                throw new DataAccessException(ERROR_LOADING_TRANSFORMATION, exception);
            }
        }
    }
}
