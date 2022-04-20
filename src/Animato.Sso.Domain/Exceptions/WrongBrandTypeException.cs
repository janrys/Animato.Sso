namespace Animato.Sso.Domain.Exceptions;

public class WrongBrandTypeException : BaseException
{
    public WrongBrandTypeException(int id) : base($"Brand type with id '{id}' is not supported.") { }
    public WrongBrandTypeException(string name) : base($"Brand type with name '{name}' is not supported.") { }
}
