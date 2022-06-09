namespace Animato.Sso.Domain.Entities;

using Animato.Sso.Domain.Enums;

public class Application
{
    public ApplicationId Id { get; set; }
    public string Name { get; set; }
    public string Code { get; set; }
    public List<string> Secrets { get; set; } = new List<string>();
    public List<string> RedirectUris { get; set; } = new List<string>();

    public int AccessTokenExpirationMinutes { get; set; }
    public int RefreshTokenExpirationMinutes { get; set; }
    public bool Use2Fa { get; set; }

    public AuthorizationType AuthorizationType { get; set; }

}

