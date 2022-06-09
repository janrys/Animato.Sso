namespace Animato.Sso.Application.Models;

using System.Text.Json.Serialization;
using Newtonsoft.Json;

public class SystemMetadata
{
    // https://openid.net/specs/openid-connect-discovery-1_0.html
    // https://datatracker.ietf.org/doc/html/rfc8414
    //"issuer":
    //  "https://server.example.com",
    //"authorization_endpoint":
    //  "https://server.example.com/authorize",
    //"token_endpoint":
    //  "https://server.example.com/token",
    //"token_endpoint_auth_methods_supported":
    //  ["client_secret_basic", "private_key_jwt"],
    //"token_endpoint_auth_signing_alg_values_supported":
    //  ["RS256", "ES256"],
    //"userinfo_endpoint":
    //  "https://server.example.com/userinfo",
    //"jwks_uri":
    //  "https://server.example.com/jwks.json",
    //"registration_endpoint":
    //  "https://server.example.com/register",
    //"scopes_supported":
    //  ["openid", "profile", "email", "address",
    //   "phone", "offline_access"],
    //"response_types_supported":
    //  ["code", "code token"],
    //"service_documentation":
    //  "http://server.example.com/service_documentation.html",
    //"ui_locales_supported":
    //  ["en-US", "en-GB", "en-CA", "fr-FR", "fr-CA"]

    [JsonProperty("issuer")]
    [JsonPropertyName("issuer")]
    public string Issuer { get; set; }

    [JsonProperty("authorization_endpoint")]
    [JsonPropertyName("authorization_endpoint")]
    public string AuthorizationEndpoint { get; set; }

    [JsonProperty("token_endpoint")]
    [JsonPropertyName("token_endpoint")]
    public string TokenEndpoint { get; set; }

    [JsonProperty("token_endpoint_auth_methods_supported")]
    [JsonPropertyName("token_endpoint_auth_methods_supported")]
    public List<string> TokenEndpointAuthMethodsSupported { get; set; }

    [JsonProperty("token_endpoint_auth_signing_alg_values_supported")]
    [JsonPropertyName("token_endpoint_auth_signing_alg_values_supported")]
    public List<string> TokenEndpointAuthSigningAlgValuesSupported { get; set; }

    [JsonProperty("userinfo_endpoint")]
    [JsonPropertyName("userinfo_endpoint")]
    public string UserInfoEndpoint { get; set; }

    [JsonProperty("revocation_endpoint")]
    [JsonPropertyName("revocation_endpoint")]
    public string RevocationEndpoint { get; set; }

    [JsonProperty("scopes_supported")]
    [JsonPropertyName("scopes_supported")]
    public List<string> ScopesSupported { get; set; }

    [JsonProperty("response_types_supported")]
    [JsonPropertyName("response_types_supported")]
    public List<string> ResponseTypesSupported { get; set; }

    [JsonProperty("service_documentation")]
    [JsonPropertyName("service_documentation")]
    public List<string> ServiceDocumentation { get; set; }

    [JsonProperty("ui_locales_supported")]
    [JsonPropertyName("ui_locales_supported")]
    public List<string> UiLocalesSupported { get; set; }
}
