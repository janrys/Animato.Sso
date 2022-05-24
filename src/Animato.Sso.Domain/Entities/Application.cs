namespace Animato.Sso.Domain.Entities;

public class Application
{
    public ApplicationId Id { get; set; }
    public string Name { get; set; }
    public string Code { get; set; }
    public List<string> Secrets { get; set; } = new List<string>();
    public List<string> RedirectUris { get; set; } = new List<string>();

}

