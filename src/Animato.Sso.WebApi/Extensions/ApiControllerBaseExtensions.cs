namespace Animato.Sso.WebApi.Extensions;

using Animato.Sso.WebApi.Common;
using Animato.Sso.WebApi.Controllers;

public static class ApiControllerBaseExtensions
{
    public static IQueryBuilder Query(this ApiControllerBase controller) => Query(controller, CancellationToken.None);
    public static IQueryBuilder Query(this ApiControllerBase controller, CancellationToken cancellationToken) => new CommandQueryBuilder(controller.Mediator, cancellationToken);
    public static IQueryBuilder QueryForCurrentUser(this ApiControllerBase controller, CancellationToken cancellationToken) => new CommandQueryBuilder(controller.Mediator, controller.User, cancellationToken);
    public static ICommandBuilder Command(this ApiControllerBase controller) => Command(controller, CancellationToken.None);
    public static ICommandBuilder Command(this ApiControllerBase controller, CancellationToken cancellationToken) => new CommandQueryBuilder(controller.Mediator, cancellationToken);
    public static ICommandBuilder CommandForCurrentUser(this ApiControllerBase controller, CancellationToken cancellationToken) => new CommandQueryBuilder(controller.Mediator, controller.User, cancellationToken);
}


