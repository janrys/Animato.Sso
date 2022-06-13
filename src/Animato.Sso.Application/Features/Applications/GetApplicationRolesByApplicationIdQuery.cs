﻿namespace Animato.Sso.Application.Features.Applications;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Animato.Sso.Application.Common.Interfaces;
using Animato.Sso.Application.Exceptions;
using Animato.Sso.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

public class GetApplicationRolesByApplicationIdQuery : IRequest<IEnumerable<ApplicationRole>>
{
    public GetApplicationRolesByApplicationIdQuery(Domain.Entities.ApplicationId applicationId, ClaimsPrincipal user)
    {
        ApplicationId = applicationId;
        User = user;
    }

    public Domain.Entities.ApplicationId ApplicationId { get; }
    public ClaimsPrincipal User { get; }

    public class GetApplicationRolesByApplicationIdQueryValidator : AbstractValidator<GetApplicationRolesByApplicationIdQuery>
    {
        public GetApplicationRolesByApplicationIdQueryValidator()
            => RuleFor(v => v.ApplicationId).NotEmpty().WithMessage(v => $"{nameof(v.ApplicationId)} must have a value");
    }

    public class GetApplicationRolesByApplicationIdQueryHandler : IRequestHandler<GetApplicationRolesByApplicationIdQuery, IEnumerable<ApplicationRole>>
    {
        private readonly IApplicationRoleRepository roleRepository;
        private readonly ILogger<GetApplicationRolesByApplicationIdQueryHandler> logger;
        private const string ERROR_LOADING_APPLICATION_ROLES = "Error loading application roles";

        public GetApplicationRolesByApplicationIdQueryHandler(IApplicationRoleRepository roleRepository, ILogger<GetApplicationRolesByApplicationIdQueryHandler> logger)
        {
            this.roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<ApplicationRole>> Handle(GetApplicationRolesByApplicationIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                return await roleRepository.GetByApplicationId(request.ApplicationId, cancellationToken);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, ERROR_LOADING_APPLICATION_ROLES);
                throw new DataAccessException(ERROR_LOADING_APPLICATION_ROLES, exception);
            }
        }
    }
}
