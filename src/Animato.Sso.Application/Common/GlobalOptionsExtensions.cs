namespace Animato.Sso.Application.Common;

using Animato.Sso.Domain.Enums;

public static class GlobalOptionsExtensions
{
    public static bool UseApplicationInsights(this GlobalOptions globalOptions)
        => !string.IsNullOrEmpty(globalOptions.ApplicationInsightsKey);

    public static HashAlgorithmType GetHashAlgorithm(this GlobalOptions globalOptions)
        => HashAlgorithmType.FromName(globalOptions.HashAlgorithmType);
}
