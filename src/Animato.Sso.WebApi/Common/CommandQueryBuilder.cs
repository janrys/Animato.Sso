namespace Animato.Sso.WebApi.Common;

using System.Security.Claims;
using Animato.Sso.Application.Features.Users;
using Animato.Sso.Application.Models;
using Animato.Sso.Domain.Entities;
using MediatR;

public interface ICommandBuilder
{
    IUserCommandBuilder User { get; }
}

public interface IUserCommandBuilder
{
    Task<AuthorizationResult> Authorize(AuthorizationRequest authorizationRequest);
}

public interface IQueryBuilder
{
    IUserQueryBuilder User { get; }
}

public interface IUserQueryBuilder
{
    Task<IEnumerable<User>> GetAll();
    Task<User> GetByUserName(string userName);
}

public class CommandQueryBuilder : ICommandBuilder, IUserCommandBuilder
    , IQueryBuilder, IUserQueryBuilder

{
    private readonly ISender mediator;
    private readonly CancellationToken cancellationToken;
    private readonly ClaimsPrincipal user;

    public CommandQueryBuilder(ISender mediator) : this(mediator, CancellationToken.None) { }
    public CommandQueryBuilder(ISender mediator, ClaimsPrincipal user) : this(mediator, user, CancellationToken.None) { }
    public CommandQueryBuilder(ISender mediator, CancellationToken cancellationToken) : this(mediator, null, cancellationToken) { }
    public CommandQueryBuilder(ISender mediator, ClaimsPrincipal user, CancellationToken cancellationToken)
    {
        this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        this.user = user;
        this.cancellationToken = cancellationToken;
    }

    IUserCommandBuilder ICommandBuilder.User => this;
    IUserQueryBuilder IQueryBuilder.User => this;

    Task<IEnumerable<User>> IUserQueryBuilder.GetAll() => mediator.Send(new GetUsersQuery(user), cancellationToken);
    Task<User> IUserQueryBuilder.GetByUserName(string userName) => mediator.Send(new GetUserByUserNameQuery(userName, user), cancellationToken);
    Task<AuthorizationResult> IUserCommandBuilder.Authorize(AuthorizationRequest authorizationRequest)
        => mediator.Send(new AuthorizeUserCommand(authorizationRequest, user), cancellationToken);
}
