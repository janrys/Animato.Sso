namespace Animato.Sso.Application.Features.Scopes;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Animato.Sso.Application.Common.Interfaces;
using Animato.Sso.Application.Common.Logging;
using Animato.Sso.Application.Exceptions;
using Animato.Sso.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

public class GetScopesQuery : IRequest<IEnumerable<Scope>>
{
    public GetScopesQuery(ClaimsPrincipal user) => User = user;

    public ClaimsPrincipal User { get; }

    public class GetScopesQueryHandler : IRequestHandler<GetScopesQuery, IEnumerable<Scope>>
    {
        private readonly IScopeRepository scopeRepository;
        private readonly ILogger<GetScopesQueryHandler> logger;

        public GetScopesQueryHandler(IScopeRepository scopeRepository, ILogger<GetScopesQueryHandler> logger)
        {
            this.scopeRepository = scopeRepository ?? throw new ArgumentNullException(nameof(scopeRepository));
            this.logger = logger;
        }

        public async Task<IEnumerable<Scope>> Handle(GetScopesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                return await scopeRepository.GetAll(cancellationToken);
            }
            catch (Exception exception)
            {
                logger.ScopesLoadingError(exception);
                throw new DataAccessException(LogMessageTexts.ErrorLoadingScopes, exception);
            }
        }
    }
}
