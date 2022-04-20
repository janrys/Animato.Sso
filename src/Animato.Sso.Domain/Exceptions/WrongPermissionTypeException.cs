namespace Animato.Sso.Domain.Exceptions;

public class WrongPermissionTypeException : BaseException
{
    public WrongPermissionTypeException(int id) : base($"Permission type with id '{id}' is not supported.") { }
    public WrongPermissionTypeException(string name) : base($"Permission type with name '{name}' is not supported.") { }
}
