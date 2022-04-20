namespace Animato.Sso.Application.Features.Partners;
using System;
using System.Threading;
using System.Threading.Tasks;
using Animato.Sso.Application.Common.Interfaces;
using Animato.Sso.Application.Exceptions;
using Animato.Sso.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

public class DeleteTransformationCommand : IRequest<Unit>
{
    public DeleteTransformationCommand(Guid id) => Id = id;

    public Guid Id { get; }

    public class DeleteTransformationCommandHandler : IRequestHandler<DeleteTransformationCommand, Unit>
    {
        private readonly ITransformationStorageService transformationStorage;
        private readonly ILogger<DeleteTransformationCommandHandler> logger;
        private const string ERROR_DELETING_TRANSFORMATION = "Error deleting transformation";

        public DeleteTransformationCommandHandler(ITransformationStorageService transformationStorage, ILogger<DeleteTransformationCommandHandler> logger)
        {
            this.transformationStorage = transformationStorage ?? throw new ArgumentNullException(nameof(DeleteTransformationCommandHandler.transformationStorage));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Unit> Handle(DeleteTransformationCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var assetMetadata = await transformationStorage.GetTransformation(request.Id, cancellationToken);

                if (assetMetadata is null)
                {
                    throw new NotFoundException(nameof(AssetMetadata), request.Id);
                }

                await transformationStorage.DeleteTransformation(request.Id, cancellationToken);

                return Unit.Value;
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, ERROR_DELETING_TRANSFORMATION);
                throw new DataAccessException(ERROR_DELETING_TRANSFORMATION, exception);
            }
        }
    }
}
