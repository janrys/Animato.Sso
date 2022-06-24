namespace Animato.Sso.WebApi.Common;

public interface ICommandBuilder
{
    IUserCommandBuilder User { get; }
    ITokenCommandBuilder Token { get; }
    IApplicationCommandBuilder Application { get; }
    IScopeCommandBuilder Scope { get; }
}
