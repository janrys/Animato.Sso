namespace Animato.Sso.Application.Features.Scopes;
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

public class CreateScopeCommand : IRequest<IEnumerable<Scope>>
{
    public CreateScopeCommand(CreateScopesModel scopes, ClaimsPrincipal user)
    {
        Scopes = scopes;
        User = user;
    }

    public CreateScopesModel Scopes { get; }
    public ClaimsPrincipal User { get; }

    public class CreateScopeCommandValidator : AbstractValidator<CreateScopeCommand>
    {
        public CreateScopeCommandValidator()
        {
            RuleFor(v => v.Scopes).NotNull().WithMessage(v => $"{nameof(v.Scopes)} must have a value");
            RuleFor(v => v.Scopes.Names).NotEmpty().Must(s => s.All(n => !string.IsNullOrEmpty(n))).WithMessage(v => $"{nameof(v.Scopes.Names)} must have a value");
        }
    }

    public class CreateScopeCommandHandler : IRequestHandler<CreateScopeCommand, IEnumerable<Scope>>
    {
        private readonly IScopeRepository scopeRepository;
        private readonly ILogger<CreateScopeCommandHandler> logger;

        public CreateScopeCommandHandler(IScopeRepository scopeRepository
            , ILogger<CreateScopeCommandHandler> logger)
        {
            this.scopeRepository = scopeRepository ?? throw new ArgumentNullException(nameof(scopeRepository));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<Scope>> Handle(CreateScopeCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var storedScopes = await scopeRepository.GetScopesByName(cancellationToken, request.Scopes.Names.ToArray());

                if (storedScopes.Any())
                {
                    throw new Exceptions.ValidationException(
                        Exceptions.ValidationException.CreateFailure("name"
                        , $"Scopes with names {string.Join(", ", storedScopes.Select(s => s.Name))} already exists"));
                }

                var scopes = new List<Scope>();
                scopes.AddRange(request.Scopes.Names.Select(n => new Scope() { Name = n }));
                return await scopeRepository.Create(cancellationToken, scopes.ToArray());
            }
            catch (Exceptions.ValidationException) { throw; }
            catch (Exception exception)
            {
                logger.ScopesCreatingError(exception);
                throw new DataAccessException(LogMessageTexts.ErrorCreatingScopes, exception);
            }
        }
    }

}

