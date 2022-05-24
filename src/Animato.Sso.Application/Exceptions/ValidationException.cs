namespace Animato.Sso.Application.Exceptions;
using Animato.Sso.Domain.Exceptions;
using FluentValidation.Results;

public class ValidationException : BaseException
{
    public ValidationException()
        : base("One or more validation failures have occurred.") => Errors = new Dictionary<string, string[]>();

    public ValidationException(IEnumerable<ValidationFailure> failures)
        : this() => Errors = failures
            .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
            .ToDictionary(failureGroup => failureGroup.Key, failureGroup => failureGroup.ToArray());


    public IDictionary<string, string[]> Errors { get; }

    public static IEnumerable<ValidationFailure> CreateFailure(string propertyName, string errorMessage)
        => new ValidationFailure[] { new ValidationFailure(propertyName, errorMessage) };
    public static IEnumerable<ValidationFailure> CreateFailures(params KeyValuePair<string, string>[] failures)
        => failures.Select(f => new ValidationFailure(f.Key, f.Value));
}
