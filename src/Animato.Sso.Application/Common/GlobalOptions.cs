namespace Animato.Sso.Application.Common;
public class GlobalOptions
{
    public const string ConfigurationKey = nameof(GlobalOptions);

    public static System.Globalization.CultureInfo Culture { get; } = System.Globalization.CultureInfo.InvariantCulture;
    public static string DatePattern { get; } = "s";

    public string Persistence { get; set; } = "inmemory";
    public string LogLevel { get; set; } = "Information";
}