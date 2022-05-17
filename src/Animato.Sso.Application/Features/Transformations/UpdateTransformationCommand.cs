namespace Animato.Sso.Application.Features.Transformations;
using System;
using System.Threading;
using System.Threading.Tasks;
using Animato.Sso.Application.Common.Interfaces;
using Animato.Sso.Application.Exceptions;
using Animato.Sso.Application.Models;
using Animato.Sso.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

public class UpdateTransformationCommand : IRequest<TransformationDefinition>
{
    public UpdateTransformationCommand(UpdateTransformationDefinition transformation) => Transformation = transformation;

    public UpdateTransformationDefinition Transformation { get; }

    public class UpdateTransformationCommandHandler : IRequestHandler<UpdateTransformationCommand, TransformationDefinition>
    {
        private readonly ITransformationStorageService storage;
        private readonly ILogger<UpdateTransformationCommandHandler> logger;
        private const string ERROR_UPDATING_TRANSFORMATION = "Error updating transformation";

        public UpdateTransformationCommandHandler(ITransformationStorageService storage, ILogger<UpdateTransformationCommandHandler> logger)
        {
            this.storage = storage ?? throw new ArgumentNullException(nameof(storage));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<TransformationDefinition> Handle(UpdateTransformationCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var transformation = await storage.GetTransformation(request.Transformation.Id, cancellationToken);

                if (transformation == null)
                {
                    throw new NotFoundException(nameof(transformation), request.Transformation.Id);
                }

                transformation.Definition = request.Transformation.Definition;
                transformation.Description = request.Transformation.Description;
                transformation = await storage.UpdateTransformation(transformation, cancellationToken);

                return transformation;
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, ERROR_UPDATING_TRANSFORMATION);
                throw new DataAccessException(ERROR_UPDATING_TRANSFORMATION, exception);
            }
        }
    }

    public class UpdateTransformationCommandValidator : AbstractValidator<UpdateTransformationCommand>
    {
        public UpdateTransformationCommandValidator()
        {
            RuleFor(v => v.Transformation).NotNull().WithMessage(v => $"{nameof(v.Transformation)} must have a value");
            RuleFor(v => v.Transformation.Id).NotEmpty().WithMessage(v => $"{nameof(v.Transformation.Id)} must have a value");
            RuleFor(v => v.Transformation.Definition).NotNull().WithMessage(v => $"{nameof(v.Transformation.Definition)} must have a value");
        }
    }
}
