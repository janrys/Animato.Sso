namespace Animato.Sso.Application.Features.Applications;
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

public class GetApplicationScopesQuery : IRequest<IEnumerable<Scope>>
{
    public GetApplicationScopesQuery(Domain.Entities.ApplicationId applicationId, ClaimsPrincipal user)
    {
        ApplicationId = applicationId;
        User = user;
    }

    public Domain.Entities.ApplicationId ApplicationId { get; }
    public ClaimsPrincipal User { get; }

    public class GetApplicationScopesQueryValidator : AbstractValidator<GetApplicationScopesQuery>
    {
        public GetApplicationScopesQueryValidator()
            => RuleFor(v => v.ApplicationId).NotEmpty().WithMessage(v => $"{nameof(v.ApplicationId)} must have a value");
    }

    public class GetApplicationScopesQueryHandler : IRequestHandler<GetApplicationScopesQuery, IEnumerable<Scope>>
    {
        private readonly IApplicationRepository applicationRepository;
        private readonly ILogger<GetApplicationScopesQueryHandler> logger;

        public GetApplicationScopesQueryHandler(IApplicationRepository applicationRepository, ILogger<GetApplicationScopesQueryHandler> logger)
        {
            this.applicationRepository = applicationRepository ?? throw new ArgumentNullException(nameof(applicationRepository));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<Scope>> Handle(GetApplicationScopesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var application = await applicationRepository.GetById(request.ApplicationId, cancellationToken);

                if (application is null)
                {
                    throw new NotFoundException(nameof(Application), request.ApplicationId);
                }

                return await applicationRepository.GetScopes(request.ApplicationId, cancellationToken);
            }
            catch (NotFoundException) { throw; }
            catch (Exception exception)
            {
                logger.ScopesLoadingError(exception);
                throw new DataAccessException(LogMessageTexts.ErrorLoadingScopes, exception);
            }
        }
    }
}
