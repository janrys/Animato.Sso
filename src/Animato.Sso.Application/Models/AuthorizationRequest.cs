namespace Animato.Sso.Application.Models;

using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

public class AuthorizationRequest
{
    [FromQuery(Name = "response_type")]
    [JsonProperty("response_type")]
    [JsonPropertyName("response_type")]
    public string ResponseType { get; set; }

    [FromQuery(Name = "client_id")]
    [JsonProperty("client_id")]
    [JsonPropertyName("client_id")]
    public string ClientId { get; set; }

    [FromQuery(Name = "redirect_uri")]
    [JsonProperty("redirect_uri")]
    [JsonPropertyName("redirect_uri")]
    public string RedirectUri { get; set; }

    [FromQuery(Name = "state")]
    [JsonProperty("state")]
    [JsonPropertyName("state")]
    public string State { get; set; }

    [FromQuery(Name = "scope")]
    [JsonProperty("scope")]
    [JsonPropertyName("scope")]
    public string Scope { get; set; }
}
