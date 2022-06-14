namespace Animato.Sso.Application.Features.Applications.DTOs;

using Animato.Sso.Domain.Enums;

public class CreateApplicationModel
{
    public string Name { get; set; }
    public string Code { get; set; }
    public List<string> Secrets { get; set; } = new List<string>();
    public List<string> RedirectUris { get; set; } = new List<string>();

    public int? AccessTokenExpirationMinutes { get; set; }
    public int? RefreshTokenExpirationMinutes { get; set; }
    public bool Use2Fa { get; set; }

    public AuthorizationMethod AuthorizationMethod { get; set; }
}
