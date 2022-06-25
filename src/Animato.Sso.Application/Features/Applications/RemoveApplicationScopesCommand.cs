namespace Animato.Sso.Application.Features.Applications;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Animato.Sso.Application.Common.Interfaces;
using Animato.Sso.Application.Common.Logging;
using Animato.Sso.Application.Exceptions;
using Animato.Sso.Application.Features.Scopes.DTOs;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

public class RemoveApplicationScopesCommand : IRequest<Unit>
{
    public RemoveApplicationScopesCommand(Domain.Entities.ApplicationId applicationId, CreateScopesModel scopes, ClaimsPrincipal user)
    {
        ApplicationId = applicationId;
        Scopes = scopes;
        User = user;
    }
    public Domain.Entities.ApplicationId ApplicationId { get; }
    public CreateScopesModel Scopes { get; }
    public ClaimsPrincipal User { get; }

    public class RemoveApplicationScopesCommandValidator : AbstractValidator<RemoveApplicationScopesCommand>
    {
        public RemoveApplicationScopesCommandValidator()
        {
            RuleFor(v => v.ApplicationId).NotNull().WithMessage(v => $"{nameof(v.ApplicationId)} must have a value");
            RuleFor(v => v.Scopes).NotEmpty().WithMessage(v => $"{nameof(v.Scopes)} must have a value");
            RuleFor(v => v.Scopes.Names).NotEmpty().WithMessage(v => $"{nameof(v.Scopes.Names)} must have a value");
            RuleFor(v => v.Scopes.Names).Must(n => n.All(n => !string.IsNullOrEmpty(n))).WithMessage(v => $"{nameof(v.Scopes)} must have a value");
        }
    }

    public class RemoveApplicationScopesCommandHandler : IRequestHandler<RemoveApplicationScopesCommand, Unit>
    {
        private readonly IApplicationRepository applicationRepository;
        private readonly IScopeRepository scopeRepository;
        private readonly ILogger<RemoveApplicationScopesCommandHandler> logger;

        public RemoveApplicationScopesCommandHandler(IApplicationRepository applicationRepository
            , IScopeRepository scopeRepository
            , ILogger<RemoveApplicationScopesCommandHandler> logger)
        {
            this.applicationRepository = applicationRepository ?? throw new ArgumentNullException(nameof(applicationRepository));
            this.scopeRepository = scopeRepository ?? throw new ArgumentNullException(nameof(scopeRepository));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Unit> Handle(RemoveApplicationScopesCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var application = await applicationRepository.GetById(request.ApplicationId, cancellationToken);

                if (application is null)
                {
                    throw new NotFoundException(nameof(Application), request.ApplicationId.Value);
                }

                var scopes = await scopeRepository.GetScopesByName(cancellationToken, request.Scopes.Names.ToArray());

                foreach (var scope in scopes)
                {
                    await applicationRepository.DeleteApplicationScope(application.Id, scope.Id, cancellationToken);
                }

                return Unit.Value;
            }
            catch (NotFoundException) { throw; }
            catch (Exceptions.ValidationException) { throw; }
            catch (Exception exception)
            {
                logger.ScopesDeletingError(exception);
                throw new DataAccessException(LogMessageTexts.ErrorDeletingScopes, exception);
            }
        }
    }

}
