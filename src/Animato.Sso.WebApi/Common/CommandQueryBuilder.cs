namespace Animato.Sso.WebApi.Common;

using System.Security.Claims;
using Animato.Sso.Application.Features.Tokens;
using Animato.Sso.Application.Features.Users;
using Animato.Sso.Application.Models;
using Animato.Sso.Domain.Entities;
using MediatR;

public interface ICommandBuilder
{
    IUserCommandBuilder User { get; }
    ITokenCommandBuilder Token { get; }
}

public interface IUserCommandBuilder
{
    Task<AuthorizationResult> Authorize(AuthorizationRequest authorizationRequest);
    Task<User> Login(string userName, string password);
    Task<TokenResult> GetToken(TokenRequest tokenRequest);
}

public interface ITokenCommandBuilder
{
    Task RevokeToken(string token);
    Task RevokeAllTokens();
}

public interface IQueryBuilder
{
    IUserQueryBuilder User { get; }
    ITokenQueryBuilder Token { get; }
}

public interface IUserQueryBuilder
{
    Task<IEnumerable<User>> GetAll();
    Task<User> GetByUserName(string userName);
}

public interface ITokenQueryBuilder
{
    Task<TokenInfo> GetTokenInfo(string token);
}

public class CommandQueryBuilder : ICommandBuilder, IUserCommandBuilder, ITokenCommandBuilder
    , IQueryBuilder, IUserQueryBuilder, ITokenQueryBuilder

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
    ITokenCommandBuilder ICommandBuilder.Token => this;
    ITokenQueryBuilder IQueryBuilder.Token => this;

    Task<IEnumerable<User>> IUserQueryBuilder.GetAll()
        => mediator.Send(new GetUsersQuery(user), cancellationToken);

    Task<User> IUserQueryBuilder.GetByUserName(string userName)
        => mediator.Send(new GetUserByUserNameQuery(userName, user), cancellationToken);

    Task<TokenInfo> ITokenQueryBuilder.GetTokenInfo(string token)
        => mediator.Send(new GetTokenInfoQuery(token), cancellationToken);


    Task<AuthorizationResult> IUserCommandBuilder.Authorize(AuthorizationRequest authorizationRequest)
        => mediator.Send(new AuthorizeUserCommand(authorizationRequest, user), cancellationToken);

    Task<User> IUserCommandBuilder.Login(string userName, string password)
        => mediator.Send(new LoginUserCommand(userName, password), cancellationToken);

    Task<TokenResult> IUserCommandBuilder.GetToken(TokenRequest tokenRequest)
        => mediator.Send(new GetTokenCommand(tokenRequest), cancellationToken);

    Task ITokenCommandBuilder.RevokeToken(string token)
        => mediator.Send(new RevokeTokenCommand(token), cancellationToken);

    Task ITokenCommandBuilder.RevokeAllTokens()
        => mediator.Send(new RevokeAllTokensCommand(user), cancellationToken);
}
