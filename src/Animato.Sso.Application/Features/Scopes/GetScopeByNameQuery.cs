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

public class GetScopeByNameQuery : IRequest<Scope>
{
    public GetScopeByNameQuery(string name, ClaimsPrincipal user)
    {
        Name = name;
        User = user;
    }

    public string Name { get; }
    public ClaimsPrincipal User { get; }

    public class GetScopeByNameQueryValidator : AbstractValidator<GetScopeByNameQuery>
    {
        public GetScopeByNameQueryValidator()
            => RuleFor(v => v.Name).NotEmpty().WithMessage(v => $"{nameof(v.Name)} must have a value");

    }

    public class GetScopeByNameQueryHandler : IRequestHandler<GetScopeByNameQuery, Scope>
    {
        private readonly IScopeRepository scopeRepository;
        private readonly ILogger<GetScopeByNameQueryHandler> logger;

        public GetScopeByNameQueryHandler(IScopeRepository scopeRepository, ILogger<GetScopeByNameQueryHandler> logger)
        {
            this.scopeRepository = scopeRepository ?? throw new ArgumentNullException(nameof(scopeRepository));
            this.logger = logger;
        }

        public async Task<Scope> Handle(GetScopeByNameQuery request, CancellationToken cancellationToken)
        {
            try
            {
                return await scopeRepository.GetScopeByName(request.Name, cancellationToken);
            }
            catch (Exception exception)
            {
                logger.ScopesLoadingError(exception);
                throw new DataAccessException(LogMessageTexts.ErrorLoadingScopes, exception);
            }
        }
    }
}
