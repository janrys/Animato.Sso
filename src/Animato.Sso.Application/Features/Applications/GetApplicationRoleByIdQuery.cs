namespace Animato.Sso.Application.Features.Applications;
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

public class GetApplicationRoleByIdQuery : IRequest<ApplicationRole>
{
    public GetApplicationRoleByIdQuery(ApplicationRoleId applicationRoleId, ClaimsPrincipal user)
    {
        ApplicationRoleId = applicationRoleId;
        User = user;
    }

    public ApplicationRoleId ApplicationRoleId { get; }
    public ClaimsPrincipal User { get; }

    public class GetApplicationRoleByIdQueryValidator : AbstractValidator<GetApplicationRoleByIdQuery>
    {
        public GetApplicationRoleByIdQueryValidator()
            => RuleFor(v => v.ApplicationRoleId).NotEmpty().WithMessage(v => $"{nameof(v.ApplicationRoleId)} must have a value");
    }

    public class GetApplicationRoleByIdQueryHandler : IRequestHandler<GetApplicationRoleByIdQuery, ApplicationRole>
    {
        private readonly IApplicationRoleRepository roleRepository;
        private readonly ILogger<GetApplicationRoleByIdQueryHandler> logger;
        private const string ERROR_LOADING_APPLICATION_ROLES = "Error loading application roles";

        public GetApplicationRoleByIdQueryHandler(IApplicationRoleRepository roleRepository, ILogger<GetApplicationRoleByIdQueryHandler> logger)
        {
            this.roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ApplicationRole> Handle(GetApplicationRoleByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                return await roleRepository.GetById(request.ApplicationRoleId, cancellationToken);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, ERROR_LOADING_APPLICATION_ROLES);
                throw new DataAccessException(ERROR_LOADING_APPLICATION_ROLES, exception);
            }
        }
    }
}
