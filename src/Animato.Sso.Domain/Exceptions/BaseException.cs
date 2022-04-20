namespace Animato.Sso.Domain.Exceptions;

public class BaseException : Exception
{
    public BaseException() { }
    public BaseException(string message) : base(message) { }
    public BaseException(string message, Exception inner) : base(message, inner) { }
    protected BaseException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    public Guid Id { get; protected set; } = Guid.NewGuid();
}
