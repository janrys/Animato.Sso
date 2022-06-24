namespace Animato.Sso.Application.Features.Scopes;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Animato.Sso.Application.Common;
using Animato.Sso.Application.Common.Interfaces;
using Animato.Sso.Application.Common.Logging;
using Animato.Sso.Application.Exceptions;
using Animato.Sso.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

public class UpdateScopeCommand : IRequest<Scope>
{
    public UpdateScopeCommand(string oldName, string newName
        , ClaimsPrincipal user)
    {
        OldName = oldName;
        NewName = newName;
        User = user;
    }

    public string OldName { get; }
    public string NewName { get; }
    public ClaimsPrincipal User { get; }

    public class UpdateScopeCommandValidator : AbstractValidator<UpdateScopeCommand>
    {
        public UpdateScopeCommandValidator()
        {
            RuleFor(v => v.OldName).NotEmpty().WithMessage(v => $"{nameof(v.OldName)} must have a value");
            RuleFor(v => v.NewName).NotEmpty().WithMessage(v => $"{nameof(v.NewName)} must have a value");
        }
    }

    public class UpdateScopeCommandHandler : IRequestHandler<UpdateScopeCommand, Scope>
    {
        private readonly IScopeRepository scopeRepository;
        private readonly ILogger<UpdateScopeCommandHandler> logger;

        public UpdateScopeCommandHandler(OidcOptions oidcOptions
            , IScopeRepository scopeRepository
            , ITokenFactory tokenFactory
            , ILogger<UpdateScopeCommandHandler> logger)
        {
            this.scopeRepository = scopeRepository ?? throw new ArgumentNullException(nameof(scopeRepository));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Scope> Handle(UpdateScopeCommand request, CancellationToken cancellationToken)
        {
            if (Scope.IsSystemScope(request.OldName))
            {
                throw new Exceptions.ValidationException(
                        Exceptions.ValidationException.CreateFailure("Name", $"System scope {request.OldName} cannot be renamed")
                        );
            }

            if (Scope.IsSystemScope(request.NewName))
            {
                throw new Exceptions.ValidationException(
                        Exceptions.ValidationException.CreateFailure("Name", $"Name {request.NewName} is a system scope and cannot be used")
                        );
            }

            try
            {
                var scopes = await scopeRepository.GetScopesByName(cancellationToken, request.OldName, request.NewName);

                if (!scopes.Any(s => s.Name.Equals(request.OldName, StringComparison.OrdinalIgnoreCase)))
                {
                    throw new NotFoundException(nameof(Application), request.OldName);
                }

                if (scopes.Any(s => s.Name.Equals(request.NewName, StringComparison.OrdinalIgnoreCase)))
                {
                    throw new Exceptions.ValidationException(
                    Exceptions.ValidationException.CreateFailure("name"
                    , $"Scope with name {request.NewName} already exists"));
                }

                return await scopeRepository.Update(request.OldName, request.NewName, cancellationToken);
            }
            catch (NotFoundException) { throw; }
            catch (Exceptions.ValidationException) { throw; }
            catch (Exception exception)
            {
                logger.ScopesUpdatingError(exception);
                throw new DataAccessException(LogMessageTexts.ErrorUpdatingScopes, exception);
            }
        }
    }

}
