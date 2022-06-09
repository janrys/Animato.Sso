namespace Animato.Sso.Application.Models;

using System.Text.Json.Serialization;
using Animato.Sso.Domain.Enums;
using Newtonsoft.Json;

public class TokenInfo
{
    [JsonProperty("active")]
    [JsonPropertyName("active")]
    public bool Active { get; set; }

    [JsonProperty("client_id")]
    [JsonPropertyName("client_id")]
    public string ClientId { get; set; }

    [JsonProperty("username")]
    [JsonPropertyName("username")]
    public string UserName { get; set; }

    [JsonProperty("scope")]
    [JsonPropertyName("scope")]
    public string Scope { get; set; }

    [JsonProperty("sub")]
    [JsonPropertyName("sub")]
    public string Sub { get; set; }

    [JsonProperty("aud")]
    [JsonPropertyName("aud")]
    public string Audience { get; set; }

    [JsonProperty("iss")]
    [JsonPropertyName("iss")]
    public string Issuer { get; set; }

    [JsonProperty("exp")]
    [JsonPropertyName("exp")]
    public long? Expiration { get; set; }

    [JsonProperty("iat")]
    [JsonPropertyName("iat")]
    public long? IssuedAt { get; set; }
}
