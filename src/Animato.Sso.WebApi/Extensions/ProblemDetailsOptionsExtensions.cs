namespace Animato.Sso.WebApi.Extensions;

using Animato.Sso.Application.Exceptions;
using Animato.Sso.Domain.Exceptions;
using Hellang.Middleware.ProblemDetails;

public static class ProblemDetailsOptionsExtensions
{
    public static void MapFluentValidationException(this ProblemDetailsOptions options) =>
        options.Map<FluentValidation.ValidationException>((ctx, ex) =>
        {
            var factory = ctx.RequestServices.GetRequiredService<ProblemDetailsFactory>();

            var errors = ex.Errors
                .GroupBy(x => x.PropertyName)
                .ToDictionary(
                    x => x.Key,
                    x => x.Select(x => x.ErrorMessage).ToArray());

            return factory.CreateValidationProblemDetails(ctx, errors);
        });

    public static void MapOperationCanceledException(this ProblemDetailsOptions options) =>
        options.Map<OperationCanceledException>((ctx, ex) =>
        {
            var factory = ctx.RequestServices.GetRequiredService<ProblemDetailsFactory>();
            return factory.CreateProblemDetails(ctx, StatusCodes.Status408RequestTimeout, "Operation was canceled");
        });

    public static void MapCustomExceptions(this ProblemDetailsOptions options)
    {
        options.Map<ValidationException>((ctx, ex) =>
        {
            var factory = ctx.RequestServices.GetRequiredService<ProblemDetailsFactory>();
            var errors = ex.Errors;
            return factory.CreateValidationProblemDetails(ctx, errors);
        });

        options.Map<DataAccessException>((ctx, ex) =>
        {
            var factory = ctx.RequestServices.GetRequiredService<ProblemDetailsFactory>();
            return factory.CreateProblemDetails(ctx, StatusCodes.Status500InternalServerError, "Data access error");
        });

        options.Map<ForbiddenAccessException>((ctx, ex) =>
        {
            var factory = ctx.RequestServices.GetRequiredService<ProblemDetailsFactory>();
            return factory.CreateProblemDetails(ctx, StatusCodes.Status403Forbidden, "Forbidden", detail: ex.Message);
        });

        options.Map<NotFoundException>((ctx, ex) =>
        {
            var factory = ctx.RequestServices.GetRequiredService<ProblemDetailsFactory>();
            return factory.CreateProblemDetails(ctx, StatusCodes.Status404NotFound, "Data not found", detail: ex.Message);
        });

        options.Map<WrongBrandTypeException>((ctx, ex) =>
        {
            var factory = ctx.RequestServices.GetRequiredService<ProblemDetailsFactory>();
            return factory.CreateProblemDetails(ctx, StatusCodes.Status400BadRequest, "Bad request", detail: ex.Message);
        });

        options.Map<WrongPermissionTypeException>((ctx, ex) =>
        {
            var factory = ctx.RequestServices.GetRequiredService<ProblemDetailsFactory>();
            return factory.CreateProblemDetails(ctx, StatusCodes.Status400BadRequest, "Bad request", detail: ex.Message);
        });
    }
}
