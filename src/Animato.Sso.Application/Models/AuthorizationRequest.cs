namespace Animato.Sso.Application.Models;

using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

public class AuthorizationRequest
{
    /// <summary>
    /// Response Type value that determines the authorization processing flow to be used, including what parameters are returned from the endpoints used
    /// </summary>
    [FromQuery(Name = "response_type")]
    [JsonProperty("response_type")]
    [JsonPropertyName("response_type")]
    public string ResponseType { get; set; }

    /// <summary>
    /// Client Identifier valid at the Authorization Server
    /// </summary>
    [FromQuery(Name = "client_id")]
    [JsonProperty("client_id")]
    [JsonPropertyName("client_id")]
    public string ClientId { get; set; }

    /// <summary>
    /// Redirection URI to which the response will be sent. This URI MUST exactly match one of the Redirection URI values for the Client pre-registered
    /// </summary>
    [FromQuery(Name = "redirect_uri")]
    [JsonProperty("redirect_uri")]
    [JsonPropertyName("redirect_uri")]
    public string RedirectUri { get; set; }

    /// <summary>
    /// Opaque value used to maintain state between the request and the callback
    /// </summary>
    [FromQuery(Name = "state")]
    [JsonProperty("state")]
    [JsonPropertyName("state")]
    public string State { get; set; }

    [FromQuery(Name = "scope")]
    [JsonProperty("scope")]
    [JsonPropertyName("scope")]
    public string Scope { get; set; }
}
