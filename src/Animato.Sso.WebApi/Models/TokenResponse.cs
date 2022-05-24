namespace Animato.Sso.WebApi.Models;

using System.Text.Json.Serialization;
using Animato.Sso.Application.Models;
using Newtonsoft.Json;

public class TokenResponse
{
    public TokenResponse(TokenResult tokenResult)
    {
        AccessToken = tokenResult.AccessToken;
        ExpiresIn = tokenResult.ExpiresIn;
        RefreshToken = tokenResult.RefreshToken;
        IdToken = tokenResult.IdToken;
    }

    [JsonProperty("access_token")]
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; }

    [JsonProperty("expires_in")]
    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    [JsonProperty("refresh_token")]
    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; }

    [JsonProperty("id_token")]
    [JsonPropertyName("id_token")]
    public string IdToken { get; set; }
}
