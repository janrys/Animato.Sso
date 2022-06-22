namespace Animato.Sso.Application.Features.Applications.DTOs;
public static class ApplicationRoleExensions
{

    public static Domain.Entities.ApplicationRole ApplyTo(this CreateApplicationRoleModel model, Domain.Entities.ApplicationRole role)
    {
        role.Name = model.Name;
        return role;
    }

    public static CreateApplicationRoleModel ValidateAndSanitize(this CreateApplicationRoleModel role)
    {
        if (string.IsNullOrEmpty(role.Name))
        {
            throw new Exceptions.ValidationException(
                Exceptions.ValidationException.CreateFailure(nameof(role.Name)
                , $"Role name cannot be empty"));
        }

        role.Name = role.Name.Trim();
        return role;
    }
}
