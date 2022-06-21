namespace Animato.Sso.Application.Common;
public class GlobalOptions
{
    public const string ConfigurationKey = nameof(GlobalOptions);

    public static System.Globalization.CultureInfo Culture { get; } = System.Globalization.CultureInfo.InvariantCulture;
    public static string DatePattern { get; } = "s";

    public string Persistence { get; set; } = "inmemory";
    public string LogLevel { get; set; } = "Information";
    public string ApplicationInsightsKey { get; set; } = "";
    public bool LogToAzureDiagnosticsStream { get; set; } = false;
    public string CorrelationHeaderName { get; set; } = "x-correlation-id";
    public string HashAlgorithmType { get; set; } = "SHA256";
}
