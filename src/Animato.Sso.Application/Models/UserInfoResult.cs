namespace Animato.Sso.Application.Models;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

public class UserInfoResult
{
    [JsonProperty("username")]
    [JsonPropertyName("username")]
    public string UserName { get; set; }

    [JsonProperty("sub")]
    [JsonPropertyName("sub")]
    public string Sub { get; set; }

    public Dictionary<string, string> Claims { get; set; } = new Dictionary<string, string>();
}
