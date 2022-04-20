namespace Animato.Sso.Application.Common.Logging;
public static class LoggerExtensions
{
    public static string ToLogString(this object item) => Newtonsoft.Json.JsonConvert.SerializeObject(item);
}
