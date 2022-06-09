namespace Animato.Sso.Application.Security;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Animato.Sso.Application.Common;
using Animato.Sso.Application.Common.Interfaces;
using Animato.Sso.Application.Models;
using Animato.Sso.Domain.Entities;
using Microsoft.IdentityModel.Tokens;
using SecurityClaims = System.Security.Claims;

public class TokenFactory : ITokenFactory
{
    private readonly OidcOptions oidcOptions;
    private readonly IClaimFactory claimFactory;
    private readonly IMetadataService metadataService;
    private static readonly char[] AlloweCharsForCode =
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();

    public TokenFactory(OidcOptions oidcOptions, IClaimFactory claimFactory, IMetadataService metadataService)
    {
        this.oidcOptions = oidcOptions ?? throw new ArgumentNullException(nameof(oidcOptions));
        this.claimFactory = claimFactory ?? throw new ArgumentNullException(nameof(claimFactory));
        this.metadataService = metadataService ?? throw new ArgumentNullException(nameof(metadataService));
    }


    public string GenerateCode() => GenerateRandomString(oidcOptions.AuthCodeLength);

    private string GenerateRandomString(int length)
    {
        var data = new byte[4 * length];
        using (var crypto = RandomNumberGenerator.Create())
        {
            crypto.GetBytes(data);
        }
        var result = new StringBuilder(length);
        for (var i = 0; i < length; i++)
        {
            var rnd = BitConverter.ToUInt32(data, i * 4);
            var idx = rnd % AlloweCharsForCode.Length;
            result.Append(AlloweCharsForCode[idx]);
        }

        return result.ToString();
    }

    public string GenerateAccessToken(User user, Application application, params ApplicationRole[] roles)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(oidcOptions.SecretKey);
        var claims = new List<SecurityClaims.Claim>(claimFactory.GenerateClaims(user, roles))
        {
            new SecurityClaims.Claim("login", user.Login)
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = metadataService.GetIssuer(),
            NotBefore = DateTime.UtcNow,
            Audience = application.Code,
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(application.AccessTokenExpirationMinutes),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }


    public string GenerateIdToken(User user, Application application, params ApplicationRole[] roles) => throw new NotImplementedException();

    public string GenerateRefreshToken(User user) => GenerateRandomString(oidcOptions.RefreshTokenLength);
    public TokenInfo GetTokenInfo(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        if (!tokenHandler.CanReadToken(token))
        {
            return new TokenInfo() { Active = false };
        }

        var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(token);
        return new TokenInfo()
        {
            Active = true,
            Audience = jwtToken.Audiences.FirstOrDefault(),
            ClientId = jwtToken.Audiences.FirstOrDefault(),
            Issuer = jwtToken.Issuer,
            UserName = jwtToken.Claims.FirstOrDefault(c => c.Type == "login")?.Value ?? "",
            IssuedAt = ((DateTimeOffset)jwtToken.IssuedAt).ToUnixTimeSeconds(),
            Expiration = ((DateTimeOffset)jwtToken.ValidTo).ToUnixTimeSeconds(),
        };
    }
}
