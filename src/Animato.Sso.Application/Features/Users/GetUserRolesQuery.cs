namespace Animato.Sso.Application.Features.Users;
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

public class GetUserRolesQuery : IRequest<IEnumerable<ApplicationRole>>
{
    public GetUserRolesQuery(UserId id, ClaimsPrincipal user)
    {
        Id = id;
        User = user;
    }

    public UserId Id { get; }
    public ClaimsPrincipal User { get; }

    public class GetUserRolesQueryValidator : AbstractValidator<GetUserRolesQuery>
    {
        public GetUserRolesQueryValidator() => RuleFor(v => v.Id).NotNull().WithMessage(v => $"{nameof(v.Id)} must have a value");
    }

    public class GetUserRolesQueryHandler : IRequestHandler<GetUserRolesQuery, IEnumerable<ApplicationRole>>
    {
        private readonly IApplicationRoleRepository applicationRoleRepository;
        private readonly ILogger<GetUserRolesQueryHandler> logger;

        public GetUserRolesQueryHandler(IUserRepository userRepository
            , IApplicationRoleRepository applicationRoleRepository
            , ILogger<GetUserRolesQueryHandler> logger)
        {
            this.applicationRoleRepository = applicationRoleRepository;
            this.logger = logger;
        }

        public async Task<IEnumerable<ApplicationRole>> Handle(GetUserRolesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                return await applicationRoleRepository.GetByUser(request.Id, cancellationToken);
            }
            catch (Exception exception)
            {
                logger.UsersLoadingError(exception);
                throw new DataAccessException(LogMessageTexts.ErrorLoadingUsers, exception);
            }
        }
    }
}
