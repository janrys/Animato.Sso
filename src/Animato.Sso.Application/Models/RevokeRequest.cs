namespace Animato.Sso.Application.Models;

using System.Text.Json.Serialization;
using Newtonsoft.Json;

public class RevokeRequest
{
    /// <summary>
    /// The token that the client wants to get revoked
    /// </summary>
    [JsonProperty("token")]
    [JsonPropertyName("token")]
    public string Token { get; set; }

    /// <summary>
    /// OPTIONAL.  A hint about the type of the token
    /// submitted for revocation.Clients MAY pass this parameter in
    /// order to help the authorization server to optimize the token
    /// lookup. Values access_token or refresh_token
    /// </summary>
    [JsonProperty("token_type_hint")]
    [JsonPropertyName("token_type_hint")]
    public string TokenTypeHint { get; set; }
}
