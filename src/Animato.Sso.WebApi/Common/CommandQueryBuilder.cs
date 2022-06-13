namespace Animato.Sso.WebApi.Common;

using System.Security.Claims;
using Animato.Sso.Application.Features.Applications;
using Animato.Sso.Application.Features.Applications.DTOs;
using Animato.Sso.Application.Features.Tokens;
using Animato.Sso.Application.Features.Users;
using Animato.Sso.Application.Features.Users.DTOs;
using Animato.Sso.Application.Models;
using Animato.Sso.Domain.Entities;
using MediatR;

public interface ICommandBuilder
{
    IUserCommandBuilder User { get; }
    ITokenCommandBuilder Token { get; }
    IApplicationCommandBuilder Application { get; }
}

public interface IUserCommandBuilder
{
    Task<AuthorizationResult> Authorize(AuthorizationRequest authorizationRequest);
    Task<User> Login(string login, string password);
    Task<TokenResult> GetToken(TokenRequest tokenRequest);
    Task<User> Create(CreateUserModel user);
    Task<User> Update(UserId userID, CreateUserModel user);
    Task Delete(UserId userID);
    Task<IEnumerable<ApplicationRole>> RemoveRole(UserId userId, ApplicationRoleId roleId);
    Task<IEnumerable<ApplicationRole>> AddRole(UserId userId, ApplicationRoleId roleId);
}

public interface ITokenCommandBuilder
{
    Task RevokeToken(string token);
    Task RevokeAllTokens();
}

public interface IApplicationCommandBuilder
{
    Task<Application> Create(CreateApplicationModel application);
    Task<Application> Update(ApplicationId applicationId, CreateApplicationModel application);
    Task Delete(ApplicationId applicationId);
    Task<ApplicationRole> CreateRole(ApplicationId applicationId, CreateApplicationRoleModel role);
    Task<ApplicationRole> UpdateRole(ApplicationRoleId roleId, CreateApplicationRoleModel role);
    Task DeleteRole(ApplicationRoleId roleId);
}

public interface IQueryBuilder
{
    IUserQueryBuilder User { get; }
    ITokenQueryBuilder Token { get; }
    IApplicationQueryBuilder Application { get; }
}

public interface IUserQueryBuilder
{
    Task<IEnumerable<User>> GetAll();
    Task<User> GetByLogin(string login);
    Task<User> GetById(UserId id);
    Task<IEnumerable<ApplicationRole>> GetRoles(UserId userId);
}

public interface ITokenQueryBuilder
{
    Task<TokenInfo> GetTokenInfo(string token);
}

public interface IApplicationQueryBuilder
{
    Task<IEnumerable<Application>> GetAll();
    Task<Application> GetByClientId(string clientId);
    Task<Application> GetById(ApplicationId applicationId);
    Task<IEnumerable<ApplicationRole>> GetRolesByApplicationId(ApplicationId applicationId);
    Task<ApplicationRole> GetRoleById(ApplicationRoleId applicationRoleId);
}

public class CommandQueryBuilder : ICommandBuilder, IUserCommandBuilder, ITokenCommandBuilder, IApplicationCommandBuilder
    , IQueryBuilder, IUserQueryBuilder, ITokenQueryBuilder, IApplicationQueryBuilder

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
    IApplicationCommandBuilder ICommandBuilder.Application => this;
    IApplicationQueryBuilder IQueryBuilder.Application => this;

    Task<IEnumerable<User>> IUserQueryBuilder.GetAll()
        => mediator.Send(new GetUsersQuery(user), cancellationToken);

    Task<User> IUserQueryBuilder.GetByLogin(string userName)
        => mediator.Send(new GetUserByUserNameQuery(userName, user), cancellationToken);

    Task<TokenInfo> ITokenQueryBuilder.GetTokenInfo(string token)
        => mediator.Send(new GetTokenInfoQuery(token), cancellationToken);


    Task<AuthorizationResult> IUserCommandBuilder.Authorize(AuthorizationRequest authorizationRequest)
        => mediator.Send(new AuthorizeUserCommand(authorizationRequest, user), cancellationToken);

    Task<User> IUserCommandBuilder.Login(string userName, string password)
        => mediator.Send(new LoginUserCommand(userName, password), cancellationToken);

    Task<User> IUserQueryBuilder.GetById(UserId id)
        => mediator.Send(new GetUserByIdQuery(id, user), cancellationToken);

    Task<TokenResult> IUserCommandBuilder.GetToken(TokenRequest tokenRequest)
        => mediator.Send(new GetTokenCommand(tokenRequest), cancellationToken);

    Task ITokenCommandBuilder.RevokeToken(string token)
        => mediator.Send(new RevokeTokenCommand(token), cancellationToken);

    Task ITokenCommandBuilder.RevokeAllTokens()
        => mediator.Send(new RevokeAllTokensCommand(user), cancellationToken);

    Task<IEnumerable<Application>> IApplicationQueryBuilder.GetAll()
        => mediator.Send(new GetAllApplicationsQuery(user), cancellationToken);

    Task<Application> IApplicationQueryBuilder.GetByClientId(string clientId)
        => mediator.Send(new GetApplicationByCodeQuery(clientId, user), cancellationToken);

    Task<Application> IApplicationCommandBuilder.Create(CreateApplicationModel application)
        => mediator.Send(new CreateApplicationCommand(application, user), cancellationToken);

    Task<Application> IApplicationCommandBuilder.Update(ApplicationId applicationId, CreateApplicationModel application)
        => mediator.Send(new UpdateApplicationCommand(applicationId, application, user), cancellationToken);

    Task IApplicationCommandBuilder.Delete(ApplicationId applicationId)
        => mediator.Send(new DeleteApplicationCommand(applicationId, user), cancellationToken);

    Task<Application> IApplicationQueryBuilder.GetById(ApplicationId applicationId)
        => mediator.Send(new GetApplicationByIdQuery(applicationId, user), cancellationToken);

    Task<User> IUserCommandBuilder.Create(CreateUserModel userModel)
        => mediator.Send(new CreateUserCommand(userModel, user), cancellationToken);

    Task<User> IUserCommandBuilder.Update(UserId userID, CreateUserModel userModel)
        => mediator.Send(new UpdateUserCommand(userID, userModel, user), cancellationToken);

    Task IUserCommandBuilder.Delete(UserId userID)
        => mediator.Send(new DeleteUserCommand(userID, user), cancellationToken);
    Task<IEnumerable<ApplicationRole>> IApplicationQueryBuilder.GetRolesByApplicationId(ApplicationId applicationId)
        => mediator.Send(new GetApplicationRolesByApplicationIdQuery(applicationId, user), cancellationToken);

    Task<ApplicationRole> IApplicationQueryBuilder.GetRoleById(ApplicationRoleId applicationRoleId)
        => mediator.Send(new GetApplicationRoleByIdQuery(applicationRoleId, user), cancellationToken);

    Task<ApplicationRole> IApplicationCommandBuilder.CreateRole(ApplicationId applicationId, CreateApplicationRoleModel role)
        => mediator.Send(new CreateApplicationRoleCommand(applicationId, role, user), cancellationToken);

    Task<ApplicationRole> IApplicationCommandBuilder.UpdateRole(ApplicationRoleId roleId, CreateApplicationRoleModel role)
        => mediator.Send(new UpdateApplicationRoleCommand(roleId, role, user), cancellationToken);

    Task IApplicationCommandBuilder.DeleteRole(ApplicationRoleId roleId)
        => mediator.Send(new DeleteApplicationRoleCommand(roleId, user), cancellationToken);
    Task<IEnumerable<ApplicationRole>> IUserCommandBuilder.RemoveRole(UserId userId, ApplicationRoleId roleId)
        => mediator.Send(new RemoveUserRoleCommand(userId, roleId, user), cancellationToken);

    Task<IEnumerable<ApplicationRole>> IUserCommandBuilder.AddRole(UserId userId, ApplicationRoleId roleId)
        => mediator.Send(new AddUserRoleCommand(userId, roleId, user), cancellationToken);
    Task<IEnumerable<ApplicationRole>> IUserQueryBuilder.GetRoles(UserId userId)
        => mediator.Send(new GetUserRolesQuery(userId, user), cancellationToken);

}
