namespace Animato.Sso.Domain.Entities;

public class ApplicationRole
{
    public ApplicationRoleId Id { get; set; }
    public ApplicationId ApplicationId { get; set; }
    public string Name { get; set; }
}

