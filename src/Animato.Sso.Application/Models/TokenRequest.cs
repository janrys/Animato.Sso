namespace Animato.Sso.Application.Models;

using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

public class TokenRequest
{
    [JsonProperty("grant_type")]
    [JsonPropertyName("grant_type")]
    public string GrantType { get; set; }

    [JsonProperty("code")]
    [JsonPropertyName("code")]
    public string Code { get; set; }

    [FromQuery(Name = "client_id")]
    [JsonProperty("client_id")]
    [JsonPropertyName("client_id")]
    public string ClientId { get; set; }

    [FromQuery(Name = "redirect_uri")]
    [JsonProperty("redirect_uri")]
    [JsonPropertyName("redirect_uri")]
    public string RedirectUri { get; set; }

    [FromQuery(Name = "client_secret")]
    [JsonProperty("client_secret")]
    [JsonPropertyName("client_secret")]
    public string ClientSecret { get; set; }

    [JsonProperty("refresh_token")]
    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; }
}
