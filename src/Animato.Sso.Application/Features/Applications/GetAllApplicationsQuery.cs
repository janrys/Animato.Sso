namespace Animato.Sso.Application.Features.Applications;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Animato.Sso.Application.Common.Interfaces;
using Animato.Sso.Application.Exceptions;
using Animato.Sso.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

public class GetAllApplicationsQuery : IRequest<IEnumerable<Application>>
{
    public GetAllApplicationsQuery(ClaimsPrincipal user) => User = user;


    public ClaimsPrincipal User { get; }

    public class GetAllApplicationsQueryHandler : IRequestHandler<GetAllApplicationsQuery, IEnumerable<Application>>
    {
        private readonly IApplicationRepository applicationRepository;
        private readonly ILogger<GetAllApplicationsQueryHandler> logger;
        private const string ERROR_LOADING_APPLICATIONS = "Error loading applications";

        public GetAllApplicationsQueryHandler(IApplicationRepository applicationRepository, ILogger<GetAllApplicationsQueryHandler> logger)
        {
            this.applicationRepository = applicationRepository ?? throw new ArgumentNullException(nameof(applicationRepository));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<Application>> Handle(GetAllApplicationsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                return await applicationRepository.GetAll(cancellationToken);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, ERROR_LOADING_APPLICATIONS);
                throw new DataAccessException(ERROR_LOADING_APPLICATIONS, exception);
            }
        }
    }

}
