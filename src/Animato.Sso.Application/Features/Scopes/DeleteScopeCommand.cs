namespace Animato.Sso.Application.Features.Scopes;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Animato.Sso.Application.Common.Interfaces;
using Animato.Sso.Application.Common.Logging;
using Animato.Sso.Application.Exceptions;
using Animato.Sso.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

public class DeleteScopeCommand : IRequest<Unit>
{
    public DeleteScopeCommand(string name, ClaimsPrincipal user)
    {
        Name = name;
        User = user;
    }

    public string Name { get; }
    public ClaimsPrincipal User { get; }

    public class DeleteScopeCommandValidator : AbstractValidator<DeleteScopeCommand>
    {
        public DeleteScopeCommandValidator()
            => RuleFor(v => v.Name).NotEmpty().WithMessage(v => $"{nameof(v.Name)} must have a value");
    }

    public class DeleteScopeCommandHandler : IRequestHandler<DeleteScopeCommand, Unit>
    {
        private readonly IScopeRepository scopeRepository;
        private readonly IClaimRepository claimRepository;
        private readonly IApplicationRepository applicationRepository;
        private readonly ILogger<DeleteScopeCommandHandler> logger;

        public DeleteScopeCommandHandler(IScopeRepository scopeRepository
            , IClaimRepository claimRepository
            , IApplicationRepository applicationRepository
            , ILogger<DeleteScopeCommandHandler> logger)
        {
            this.scopeRepository = scopeRepository ?? throw new ArgumentNullException(nameof(scopeRepository));
            this.claimRepository = claimRepository ?? throw new ArgumentNullException(nameof(claimRepository));
            this.applicationRepository = applicationRepository ?? throw new ArgumentNullException(nameof(applicationRepository));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Unit> Handle(DeleteScopeCommand request, CancellationToken cancellationToken)
        {
            if (Scope.IsSystemScope(request.Name))
            {
                throw new Exceptions.ValidationException(
                        Exceptions.ValidationException.CreateFailure(request.Name, $"System scope {request.Name} cannot be deleted")
                        );
            }

            try
            {
                var scope = await scopeRepository.GetScopeByName(request.Name, cancellationToken);

                if (scope is null)
                {
                    return Unit.Value;
                }

                var claims = await claimRepository.GetClaimsByScope(scope.Name, 1, cancellationToken);
                if (claims.Any())
                {
                    throw new Exceptions.ValidationException(
                        Exceptions.ValidationException.CreateFailure("Scope", $"Scope is in used by claims.")
                        );
                }

                await applicationRepository.DeleteApplicationScope(scope.Id, cancellationToken);
                await scopeRepository.Delete(scope.Name, cancellationToken);
                return Unit.Value;
            }
            catch (Exceptions.ValidationException) { throw; }
            catch (Exception exception)
            {
                logger.ScopesDeletingError(exception);
                throw new DataAccessException(LogMessageTexts.ErrorDeletingScopes, exception);
            }
        }
    }

}
