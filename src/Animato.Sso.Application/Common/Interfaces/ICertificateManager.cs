namespace Animato.Sso.Application.Common.Interfaces;
using Microsoft.IdentityModel.Tokens;

public interface ICertificateManager
{
    SecurityKey GetTokenSigningKey();
    string GetTokenSigningAlghorithm();
    Models.JsonWebKey GetJsonWebKey();
}
