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

public class CreateAssetCommand : IRequest<AssetMetadata>
{
    public CreateAssetCommand(CreateAsset asset) => Asset = asset;

    public CreateAsset Asset { get; }

    public class CreateAssetCommandHandler : IRequestHandler<CreateAssetCommand, AssetMetadata>
    {
        private readonly IMetadataStorageService metadataStorage;
        private readonly ITransformationStorageService transformationStorage;
        private readonly IFileStorageService fileStorage;
        private readonly IDomainEventService eventService;
        private readonly IAssetTransformationFactory assetTransformationFactory;
        private readonly ILogger<CreateAssetCommandHandler> logger;
        private const string ERROR_CREATING_ASSET = "Error creating asset";

        public CreateAssetCommandHandler(IMetadataStorageService metadataStorage, ITransformationStorageService transformationStorage, IFileStorageService fileStorage, IDomainEventService eventService, IAssetTransformationFactory assetTransformationFactory, ILogger<CreateAssetCommandHandler> logger)
        {
            this.metadataStorage = metadataStorage ?? throw new ArgumentNullException(nameof(metadataStorage));
            this.transformationStorage = transformationStorage ?? throw new ArgumentNullException(nameof(transformationStorage));
            this.fileStorage = fileStorage ?? throw new ArgumentNullException(nameof(fileStorage));
            this.eventService = eventService ?? throw new ArgumentNullException(nameof(eventService));
            this.assetTransformationFactory = assetTransformationFactory ?? throw new ArgumentNullException(nameof(assetTransformationFactory));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<AssetMetadata> Handle(CreateAssetCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var assetMetadata = new AssetMetadata
                {
                    ContentType = request.Asset.ContentType
                };
                var assetLocation = await fileStorage.Save(request.Asset.Stream, assetMetadata.Id, request.Asset.Name, request.Asset.ContentType, cancellationToken);
                assetMetadata.Variants.Add(new AssetVariant() { Id = assetMetadata.Id, Name = request.Asset.Name, Path = assetLocation.Path, Url = assetLocation.Url, Transformation = "" });

                await ApplyTransformations(request, assetMetadata, cancellationToken);

                assetMetadata = await metadataStorage.InsertAsset(assetMetadata, cancellationToken);
                await eventService.Publish(new AssetCreatedEvent(assetMetadata), cancellationToken);
                return assetMetadata;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, ERROR_CREATING_ASSET);
                throw new DataAccessException(ERROR_CREATING_ASSET, exception);
            }
        }

        private async Task ApplyTransformations(CreateAssetCommand request, AssetMetadata assetMetadata, CancellationToken cancellationToken)
        {
            var transformationDefinitions = new List<string>();
            if (request.Asset.TransformationId.HasValue)
            {
                var transformationDefinition = await transformationStorage.GetTransformation(request.Asset.TransformationId.Value, cancellationToken);
                if (transformationDefinition is null)
                {
                    throw new NotFoundException(nameof(TransformationDefinition), request.Asset.TransformationId.Value);
                }

                transformationDefinitions.AddRange(transformationDefinition.GetTransformations());
            }
            else
            {
                transformationDefinitions.AddRange(request.Asset.Transformations);
            }

            var transformTasks = new List<Task<AssetVariant>>();
            foreach (var definition in transformationDefinitions)
            {
                var transformation = assetTransformationFactory.GetTransformations(request.Asset.ContentType, definition).FirstOrDefault();

                if (transformation is not null)
                {
                    var parameters = GetParameters(definition);
                    transformTasks.Add(Transform(transformation, parameters, request.Asset.Stream, assetMetadata.Id, request.Asset.Name, request.Asset.ContentType, cancellationToken));
                }
            }

            if (transformTasks.Any())
            {
                await Task.WhenAll(transformTasks);
                assetMetadata.Variants.AddRange(transformTasks.Where(t => t.IsCompletedSuccessfully).Select(t => t.Result));
            }
        }

        private string GetParameters(string definition)
        {
            if (string.IsNullOrEmpty(definition)
                || !definition.Contains('(', StringComparison.OrdinalIgnoreCase)
                || !definition.Contains(')', StringComparison.OrdinalIgnoreCase))
            {
                return definition;
            }

            return definition[(definition.IndexOf('(') + 1)..definition.IndexOf(')')];
        }

        private async Task<AssetVariant> Transform(IAssetTransformation transformation, string parameters, Stream stream, Guid id, string name, string contentType, CancellationToken cancellationToken)
        {
            var transformedStream = await transformation.Transform(stream, parameters);
            var transformedFileName = $"{transformation.Code}_{name}";
            var assetLocation = await fileStorage.Save(transformedStream, id, transformedFileName, contentType, cancellationToken);
            return new AssetVariant() { Id = Guid.NewGuid(), Name = transformedFileName, Path = assetLocation.Path, Url = assetLocation.Url, Transformation = transformation.Code };
        }
    }

    public class CreateAssetCommandValidator : AbstractValidator<CreateAssetCommand>
    {
        public CreateAssetCommandValidator()
        {
            RuleFor(v => v.Asset).NotNull().WithMessage(v => $"{nameof(v.Asset)} must have a value");
            RuleFor(v => v.Asset.Name).NotEmpty().WithMessage(v => $"{nameof(v.Asset.Name)} must have a value");
            RuleFor(v => v.Asset.Stream).NotNull().WithMessage(v => $"{nameof(v.Asset.Stream)} must have a value");
        }

    }
}
