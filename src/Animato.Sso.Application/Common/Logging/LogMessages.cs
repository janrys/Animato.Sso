namespace Animato.Sso.Application.Common.Logging;
using Microsoft.Extensions.Logging;


public static partial class LogMessages
{
    /* TRACE >= 0  */

    /* DEBUG >= 5000  */

    /* INFORMATION >= 10000  */
    [LoggerMessage(10001, LogLevel.Information, "Using {LayerName} persistence layer")]
    public static partial void PersistenceLayerLoadingInformation(this ILogger logger, string layerName);

    [LoggerMessage(10002, LogLevel.Information, "{ApplicationName} seeded with id {ApplicationId} and client id {ClientId}")]
    public static partial void SsoSeededInformation(this ILogger logger, string applicationName, string applicationId, string clientId);

    [LoggerMessage(10003, LogLevel.Information, "SSO admin seeded. Change password ASAP. Login {Login}, id {Id}, password {Password}, TOTP secret key {TotpSecretKey}")]
    public static partial void SsoAdminSeededInformation(this ILogger logger, string login, string id, string password, string totpSecretKey);

    [LoggerMessage(10004, LogLevel.Information, "{DomainName} seeded")]
    public static partial void DataSeededInformation(this ILogger logger, string domainName);

    [LoggerMessage(10005, LogLevel.Information, "{ServiceName} starting")]
    public static partial void ServiceStartingInformation(this ILogger logger, string serviceName);

    [LoggerMessage(10006, LogLevel.Information, "{ServiceName} finished")]
    public static partial void ServiceFinishedInformation(this ILogger logger, string serviceName);

    /* WARNINGS >= 15000  */

    /* ERRORS >= 20000  */

    [LoggerMessage(20001, LogLevel.Error, "Error loading applications")]
    public static partial void ApplicationsLoadingError(this ILogger logger, Exception exception);

    [LoggerMessage(20002, LogLevel.Error, "Error creating applications")]
    public static partial void ApplicationsCreatingError(this ILogger logger, Exception exception);

    [LoggerMessage(20003, LogLevel.Error, "Error updating applications")]
    public static partial void ApplicationsUpdatingError(this ILogger logger, Exception exception);

    [LoggerMessage(20004, LogLevel.Error, "Error deleting applications")]
    public static partial void ApplicationsDeletingError(this ILogger logger, Exception exception);

    [LoggerMessage(20005, LogLevel.Error, LogMessageTexts.ErrorLoadingRoles)]
    public static partial void ApplicationRolesLoadingError(this ILogger logger, Exception exception);

    [LoggerMessage(20006, LogLevel.Error, LogMessageTexts.ErrorCreatingRoles)]
    public static partial void ApplicationRolesCreatingError(this ILogger logger, Exception exception);

    [LoggerMessage(20007, LogLevel.Error, LogMessageTexts.ErrorUpdatingRoles)]
    public static partial void ApplicationRolesUpdatingError(this ILogger logger, Exception exception);

    [LoggerMessage(20008, LogLevel.Error, LogMessageTexts.ErrorDeletingRoles)]
    public static partial void ApplicationRolesDeletingError(this ILogger logger, Exception exception);

    [LoggerMessage(20009, LogLevel.Error, LogMessageTexts.ErrorLoadingCodes)]
    public static partial void CodesLoadingError(this ILogger logger, Exception exception);

    [LoggerMessage(20010, LogLevel.Error, LogMessageTexts.ErrorCreatingCodes)]
    public static partial void CodesCreatingError(this ILogger logger, Exception exception);

    [LoggerMessage(20011, LogLevel.Error, LogMessageTexts.ErrorDeletingCodes)]
    public static partial void CodesDeletingError(this ILogger logger, Exception exception);

    [LoggerMessage(20012, LogLevel.Error, LogMessageTexts.ErrorLoadingUsers)]
    public static partial void UsersLoadingError(this ILogger logger, Exception exception);

    [LoggerMessage(20013, LogLevel.Error, LogMessageTexts.ErrorCreatingUsers)]
    public static partial void UsersCreatingError(this ILogger logger, Exception exception);

    [LoggerMessage(20014, LogLevel.Error, LogMessageTexts.ErrorUpdatingUsers)]
    public static partial void UsersUpdatingError(this ILogger logger, Exception exception);

    [LoggerMessage(20015, LogLevel.Error, LogMessageTexts.ErrorDeletingUsers)]
    public static partial void UsersDeletingError(this ILogger logger, Exception exception);

    [LoggerMessage(20016, LogLevel.Error, LogMessageTexts.ErrorLoadingTokens)]
    public static partial void TokensLoadingError(this ILogger logger, Exception exception);

    [LoggerMessage(20017, LogLevel.Error, LogMessageTexts.ErrorCreatingTokens)]
    public static partial void TokensCreatingError(this ILogger logger, Exception exception);

    [LoggerMessage(20018, LogLevel.Error, LogMessageTexts.ErrorUpdatingTokens)]
    public static partial void TokensUpdatingError(this ILogger logger, Exception exception);

    [LoggerMessage(20019, LogLevel.Error, LogMessageTexts.ErrorDeletingTokens)]
    public static partial void TokensDeletingError(this ILogger logger, Exception exception);

    [LoggerMessage(20020, LogLevel.Error, LogMessageTexts.ErrorLoadingClaims)]
    public static partial void ClaimsLoadingError(this ILogger logger, Exception exception);

    [LoggerMessage(20021, LogLevel.Error, LogMessageTexts.ErrorCreatingClaims)]
    public static partial void ClaimsCreatingError(this ILogger logger, Exception exception);

    [LoggerMessage(20022, LogLevel.Error, LogMessageTexts.ErrorUpdatingClaims)]
    public static partial void ClaimsUpdatingError(this ILogger logger, Exception exception);

    [LoggerMessage(20023, LogLevel.Error, LogMessageTexts.ErrorDeletingClaims)]
    public static partial void ClaimsDeletingError(this ILogger logger, Exception exception);

    [LoggerMessage(20024, LogLevel.Error, LogMessageTexts.ErrorLoadingScopes)]
    public static partial void ScopesLoadingError(this ILogger logger, Exception exception);

    [LoggerMessage(20025, LogLevel.Error, LogMessageTexts.ErrorCreatingScopes)]
    public static partial void ScopesCreatingError(this ILogger logger, Exception exception);

    [LoggerMessage(20026, LogLevel.Error, LogMessageTexts.ErrorUpdatingScopes)]
    public static partial void ScopesUpdatingError(this ILogger logger, Exception exception);

    [LoggerMessage(20027, LogLevel.Error, LogMessageTexts.ErrorDeletingScopes)]
    public static partial void ScopesDeletingError(this ILogger logger, Exception exception);

    /* CRITICAL >= 30000  */


}
