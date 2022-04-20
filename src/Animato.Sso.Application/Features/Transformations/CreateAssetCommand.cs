using Animato.Sso.Application.Features.Assets;

namespace Animato.Sso.Application.Features.Transformations;
using System;
using System.Threading;
using System.Threading.Tasks;
using Animato.Sso.Application.Common.Interfaces;
using Animato.Sso.Application.Exceptions;
using Animato.Sso.Application.Models;
using Animato.Sso.Domain.Entities;
using Animato.Sso.Domain.Enums;
using Animato.Sso.Domain.Events;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

public class CreateTransformationCommand : IRequest<TransformationDefinition>
{
    public CreateTransformationCommand(CreateTransformationDefinition transformation) => Transformation = transformation;

    public CreateTransformationDefinition Transformation { get; }

    public class CreateTransformationCommandHandler : IRequestHandler<CreateTransformationCommand, TransformationDefinition>
    {
        private readonly ITransformationStorageService transformationStorage;
        private readonly ILogger<CreateTransformationCommandHandler> logger;
        private const string ERROR_CREATING_TRANSFORMATION = "Error creating transformation";

        public CreateTransformationCommandHandler(ITransformationStorageService transformationStorage, ILogger<CreateTransformationCommandHandler> logger)
        {
            this.transformationStorage = transformationStorage ?? throw new ArgumentNullException(nameof(transformationStorage));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<TransformationDefinition> Handle(CreateTransformationCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var transformation = new TransformationDefinition()
                {
                    Id = Guid.NewGuid(),
                    Definition = request.Transformation.Definition,
                    Description = request.Transformation.Description
                };

                transformation = await transformationStorage.InsertTransformation(transformation, cancellationToken);
                return transformation;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, ERROR_CREATING_TRANSFORMATION);
                throw new DataAccessException(ERROR_CREATING_TRANSFORMATION, exception);
            }
        }

    }

    public class CreateTransformationCommandValidator : AbstractValidator<CreateTransformationCommand>
    {
        public CreateTransformationCommandValidator()
        {
            RuleFor(v => v.Transformation).NotNull().WithMessage(v => $"{nameof(v.Transformation)} must have a value");
            RuleFor(v => v.Transformation.Definition).NotEmpty().WithMessage(v => $"{nameof(v.Transformation.Definition)} must have a value");
        }

    }
}
