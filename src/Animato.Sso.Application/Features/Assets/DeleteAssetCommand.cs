namespace Animato.Sso.Application.Features.Partners;
using System;
using System.Threading;
using System.Threading.Tasks;
using Animato.Sso.Application.Common.Interfaces;
using Animato.Sso.Application.Exceptions;
using Animato.Sso.Domain.Entities;
using Animato.Sso.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

public class DeleteAssetCommand : IRequest<Unit>
{
    public DeleteAssetCommand(Guid id) => Id = id;

    public Guid Id { get; }

    public class DeleteAssetCommandHandler : IRequestHandler<DeleteAssetCommand, Unit>
    {
        private readonly IMetadataStorageService metadataStorage;
        private readonly IFileStorageService fileStorage;
        private readonly IDomainEventService eventService;
        private readonly ILogger<DeleteAssetCommandHandler> logger;
        private const string ERROR_DELETING_ASSET = "Error deleting asset";

        public DeleteAssetCommandHandler(IMetadataStorageService metadataStorage, IFileStorageService fileStorage, IDomainEventService eventService, ILogger<DeleteAssetCommandHandler> logger)
        {
            this.metadataStorage = metadataStorage ?? throw new ArgumentNullException(nameof(metadataStorage));
            this.fileStorage = fileStorage ?? throw new ArgumentNullException(nameof(fileStorage));
            this.eventService = eventService ?? throw new ArgumentNullException(nameof(eventService));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Unit> Handle(DeleteAssetCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var assetMetadata = await metadataStorage.GetAsset(request.Id, cancellationToken);

                if (assetMetadata is null)
                {
                    throw new NotFoundException(nameof(AssetMetadata), request.Id);
                }

                await fileStorage.Delete(request.Id, cancellationToken);
                await metadataStorage.DeleteAsset(request.Id, cancellationToken);

                await eventService.Publish(new AssetDeletedEvent(assetMetadata), cancellationToken);

                return Unit.Value;
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, ERROR_DELETING_ASSET);
                throw new DataAccessException(ERROR_DELETING_ASSET, exception);
            }
        }
    }
}
