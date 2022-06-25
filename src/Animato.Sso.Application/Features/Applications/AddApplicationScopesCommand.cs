namespace Animato.Sso.Application.Features.Applications;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Animato.Sso.Application.Common.Interfaces;
using Animato.Sso.Application.Common.Logging;
using Animato.Sso.Application.Exceptions;
using Animato.Sso.Application.Features.Scopes.DTOs;
using Animato.Sso.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

public class AddApplicationScopesCommand : IRequest<Unit>
{
    public AddApplicationScopesCommand(Domain.Entities.ApplicationId applicationId, CreateScopesModel scopes, ClaimsPrincipal user)
    {
        ApplicationId = applicationId;
        Scopes = scopes;
        User = user;
    }

    public Domain.Entities.ApplicationId ApplicationId { get; }
    public CreateScopesModel Scopes { get; }
    public ClaimsPrincipal User { get; }

    public class AddApplicationScopesCommandValidator : AbstractValidator<AddApplicationScopesCommand>
    {
        public AddApplicationScopesCommandValidator()
        {
            RuleFor(v => v.ApplicationId).NotNull().WithMessage(v => $"{nameof(v.ApplicationId)} must have a value");
            RuleFor(v => v.Scopes).NotEmpty().WithMessage(v => $"{nameof(v.Scopes)} must have a value");
            RuleFor(v => v.Scopes.Names).NotEmpty().WithMessage(v => $"{nameof(v.Scopes.Names)} must have a value");
            RuleFor(v => v.Scopes.Names).Must(n => n.All(n => !string.IsNullOrEmpty(n))).WithMessage(v => $"{nameof(v.Scopes)} must have a value");
        }
    }

    public class AddApplicationScopesCommandHandler : IRequestHandler<AddApplicationScopesCommand, Unit>
    {
        private readonly IApplicationRepository applicationRepository;
        private readonly IScopeRepository scopeRepository;
        private readonly ILogger<AddApplicationScopesCommandHandler> logger;

        public AddApplicationScopesCommandHandler(IApplicationRepository applicationRepository
            , IScopeRepository scopeRepository
            , ILogger<AddApplicationScopesCommandHandler> logger)
        {
            this.applicationRepository = applicationRepository ?? throw new ArgumentNullException(nameof(applicationRepository));
            this.scopeRepository = scopeRepository ?? throw new ArgumentNullException(nameof(scopeRepository));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Unit> Handle(AddApplicationScopesCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var application = await applicationRepository.GetById(request.ApplicationId, cancellationToken);

                if (application is null)
                {
                    throw new NotFoundException(nameof(Application)
                        , $"Application with id {request.ApplicationId} does not exist");
                }

                var scopes = await scopeRepository.GetScopesByName(cancellationToken, request.Scopes.Names.ToArray());

                if (!scopes.Any())
                {
                    throw new NotFoundException(nameof(Scope)
                        , $"Scopes with name {string.Join(", ", request.Scopes.Names)} do not exist");
                }

                await applicationRepository.CreateApplicationScopes(request.ApplicationId
                    , cancellationToken
                    , scopes.Select(s => s.Id).ToArray());

                return Unit.Value;
            }
            catch (NotFoundException) { throw; }
            catch (Exceptions.ValidationException) { throw; }
            catch (Exception exception)
            {
                logger.ScopesCreatingError(exception);
                throw new DataAccessException(LogMessageTexts.ErrorCreatingScopes, exception);
            }
        }
    }

}

