namespace Animato.Sso.Application.Features.Assets;
using System;
using System.Threading;
using System.Threading.Tasks;
using Animato.Sso.Application.Common.Interfaces;
using Animato.Sso.Application.Exceptions;
using Animato.Sso.Application.Models;
using Animato.Sso.Domain.Entities;
using Animato.Sso.Domain.Events;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

public class UpdateAssetCommand : IRequest<AssetMetadata>
{
    public UpdateAssetCommand(UpdateAsset updateAsset) => Asset = updateAsset;

    public UpdateAsset Asset { get; }

    public class UpdateAssetCommandHandler : IRequestHandler<UpdateAssetCommand, AssetMetadata>
    {
        private readonly IMetadataStorageService storage;
        private readonly IDomainEventService eventService;
        private readonly ILogger<UpdateAssetCommandHandler> logger;
        private const string ERROR_UPDATING_ASSET = "Error updating asset";

        public UpdateAssetCommandHandler(IMetadataStorageService storage, IDomainEventService eventService, ILogger<UpdateAssetCommandHandler> logger)
        {
            this.storage = storage ?? throw new ArgumentNullException(nameof(storage));
            this.eventService = eventService ?? throw new ArgumentNullException(nameof(eventService));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<AssetMetadata> Handle(UpdateAssetCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var assetMetadata = await storage.GetAsset(request.Asset.Id, cancellationToken);

                if (assetMetadata == null)
                {
                    throw new NotFoundException(nameof(Asset), request.Asset.Id);
                }

                assetMetadata = await storage.UpdateAsset(assetMetadata, cancellationToken);

                await eventService.Publish(new AssetUpdatedEvent(assetMetadata), cancellationToken);

                return assetMetadata;
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, ERROR_UPDATING_ASSET);
                throw new DataAccessException(ERROR_UPDATING_ASSET, exception);
            }
        }
    }

    public class UpdateAssetCommandValidator : AbstractValidator<UpdateAssetCommand>
    {
        public UpdateAssetCommandValidator()
        {
            RuleFor(v => v.Asset).NotNull().WithMessage(v => $"{nameof(v.Asset)} must have a value");
            RuleFor(v => v.Asset.Name).NotEmpty().WithMessage(v => $"{nameof(v.Asset.Name)} must have a value");
            RuleFor(v => v.Asset.Stream).NotNull().WithMessage(v => $"{nameof(v.Asset.Stream)} must have a value");
        }
    }
}
