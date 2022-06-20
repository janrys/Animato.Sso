namespace Animato.Sso.Application.Common.Logging;
using Microsoft.Extensions.Logging;


public static partial class LogMessages
{   /* TRACE >= 0  */

    /* DEBUG >= 5000  */

    /* INFORMATION >= 10000  */
    [LoggerMessage(10001, LogLevel.Information, "Using {LayerName} persistence layer")]
    public static partial void PersistenceLayerLoadingInformation(this ILogger logger, string layerName);

    /* WARNINGS >= 15000  */

    /* ERRORS >= 20000  */

    [LoggerMessage(20001, LogLevel.Error, "Error loading applications")]
    public static partial void ApplicationsLoadingError(this ILogger logger, Exception exception);

    [LoggerMessage(20002, LogLevel.Error, "Error inserting applications")]
    public static partial void ApplicationsInsertingError(this ILogger logger, Exception exception);

    [LoggerMessage(20003, LogLevel.Error, "Error updating applications")]
    public static partial void ApplicationsUpdatingError(this ILogger logger, Exception exception);

    [LoggerMessage(20004, LogLevel.Error, "Error deleting applications")]
    public static partial void ApplicationsDeletingError(this ILogger logger, Exception exception);

    [LoggerMessage(20005, LogLevel.Error, "Error loading roles")]
    public static partial void ApplicationRolesLoadingError(this ILogger logger, Exception exception);

    [LoggerMessage(20006, LogLevel.Error, "Error inserting roles")]
    public static partial void ApplicationRolesInsertingError(this ILogger logger, Exception exception);

    [LoggerMessage(20007, LogLevel.Error, "Error updating roles")]
    public static partial void ApplicationRolesUpdatingError(this ILogger logger, Exception exception);

    [LoggerMessage(20008, LogLevel.Error, "Error deleting roles")]
    public static partial void ApplicationRolesDeletingError(this ILogger logger, Exception exception);

    [LoggerMessage(20009, LogLevel.Error, "Error loading codes")]
    public static partial void CodesLoadingError(this ILogger logger, Exception exception);

    [LoggerMessage(20010, LogLevel.Error, "Error inserting codes")]
    public static partial void CodesInsertingError(this ILogger logger, Exception exception);

    [LoggerMessage(20011, LogLevel.Error, "Error deleting codes")]
    public static partial void CodesDeletingError(this ILogger logger, Exception exception);

    [LoggerMessage(20012, LogLevel.Error, "Error loading users")]
    public static partial void UsersLoadingError(this ILogger logger, Exception exception);

    [LoggerMessage(20013, LogLevel.Error, "Error inserting users")]
    public static partial void UsersInsertingError(this ILogger logger, Exception exception);

    [LoggerMessage(20014, LogLevel.Error, "Error updating users")]
    public static partial void UsersUpdatingError(this ILogger logger, Exception exception);

    [LoggerMessage(20015, LogLevel.Error, "Error deleting users")]
    public static partial void UsersDeletingError(this ILogger logger, Exception exception);

    [LoggerMessage(20016, LogLevel.Error, "Error loading tokens")]
    public static partial void TokensLoadingError(this ILogger logger, Exception exception);

    [LoggerMessage(20017, LogLevel.Error, "Error inserting tokens")]
    public static partial void TokensInsertingError(this ILogger logger, Exception exception);

    [LoggerMessage(20018, LogLevel.Error, "Error updating tokens")]
    public static partial void TokensUpdatingError(this ILogger logger, Exception exception);

    [LoggerMessage(20019, LogLevel.Error, "Error deleting tokens")]
    public static partial void TokensDeletingError(this ILogger logger, Exception exception);

    [LoggerMessage(20020, LogLevel.Error, "Error inserting claims")]
    public static partial void ClaimsInsertingError(this ILogger logger, Exception exception);

    [LoggerMessage(20021, LogLevel.Error, "Error inserting scopes")]
    public static partial void ScopesInsertingError(this ILogger logger, Exception exception);

    /* CRITICAL >= 30000  */


}
