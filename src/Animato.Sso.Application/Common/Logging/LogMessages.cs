namespace Animato.Sso.Application.Common.Logging;
using Microsoft.Extensions.Logging;

public static partial class LogMessages
{
    [LoggerMessage(0, LogLevel.Error, "Error loading users")]
    public static partial void UsersLoadingError(this ILogger logger, Exception exception);
}
