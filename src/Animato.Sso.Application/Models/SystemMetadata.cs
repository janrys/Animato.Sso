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

    [JsonProperty("minimal_password_length")]
    [JsonPropertyName("minimal_password_length")]
    public int MinimalPasswordLength { get; set; }

    [JsonProperty("correlation_header_name")]
    [JsonPropertyName("correlation_header_name")]
    public string CorrelationHeaderName { get; set; }

    [JsonProperty("authentication_code_expiration")]
    [JsonPropertyName("authentication_code_expiration")]
    public int AuthenticationCodeExpiration { get; set; }

    [JsonProperty("jwks_uri")]
    [JsonPropertyName("jwks_uri")]
    public string JsonWebKeySetUri { get; set; }
}


public class JsonWebKeySetMetadata
{
    public List<JsonWebKey> JsonWebKeys { get; set; } = new List<JsonWebKey>();
}

public class JsonWebKey
{
    public string Kty { get; set; }
    public string N { get; set; }
    public string E { get; set; }
    public string Alg { get; set; }
    public string Kid { get; set; }
    public string Use { get; set; }
}
