namespace Animato.Sso.Infrastructure.Transformations;

using System.Collections.Generic;

public class DesignifyOptions
{
    public const string CONFIGURATION_KEY = "Designify";

    public string ApiKey { get; set; }
    public object Url { get; set; }
}
