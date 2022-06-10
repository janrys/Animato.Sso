namespace Animato.Sso.Application.Common;
public class OidcOptions
{
    public const string ConfigurationKey = nameof(OidcOptions);
    public int AuthCodeLength { get; set; } = 30;
    public int ApplicationSecretLength { get; set; } = 20;
    public string SecretKey { get; set; } = "some_secret_keyxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx";
    public int AccessTokenExpirationMinutes { get; set; } = 60;
    public int RefreshTokenExpirationMinutes { get; set; } = 60 * 24;
    public int CodeExpirationMinutes { get; set; } = 10;
    public string Issuer { get; set; } = "Animato.Sso";
    public int RefreshTokenLength { get; set; } = 60;
}
