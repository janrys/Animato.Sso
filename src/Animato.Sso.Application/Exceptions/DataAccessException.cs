namespace Animato.Sso.Application.Exceptions;

using Animato.Sso.Domain.Exceptions;

public class DataAccessException : BaseException
{
    public DataAccessException(string message) : base(message) { }
    public DataAccessException(string message, Exception inner) : base(message, inner) { }
}
