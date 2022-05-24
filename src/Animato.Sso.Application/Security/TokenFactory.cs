namespace Animato.Sso.Application.Security;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Animato.Sso.Application.Common;
using Animato.Sso.Application.Common.Interfaces;
using Animato.Sso.Domain.Entities;
using Microsoft.IdentityModel.Tokens;
using SecurityClaims = System.Security.Claims;

public class TokenFactory : ITokenFactory
{
    private readonly OidcOptions oidcOptions;
    private readonly IClaimFactory claimFactory;
    private static readonly char[] AlloweCharsForCode =
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();

    public TokenFactory(OidcOptions oidcOptions, IClaimFactory claimFactory)
    {
        this.oidcOptions = oidcOptions ?? throw new ArgumentNullException(nameof(oidcOptions));
        this.claimFactory = claimFactory ?? throw new ArgumentNullException(nameof(claimFactory));
    }


    public string GenerateCode()
    {
        var data = new byte[4 * oidcOptions.AuthCodeLength];
        using (var crypto = RandomNumberGenerator.Create())
        {
            crypto.GetBytes(data);
        }
        var result = new StringBuilder(oidcOptions.AuthCodeLength);
        for (var i = 0; i < oidcOptions.AuthCodeLength; i++)
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
            Issuer = oidcOptions.Issuer,
            NotBefore = DateTime.UtcNow,
            Audience = application.Code,
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(oidcOptions.AccessTokenExpirationMinutes),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }


    public string GenerateIdToken(User user, Application application, params ApplicationRole[] roles) => throw new NotImplementedException();

    public string GenerateRefreshToken(User user) => throw new NotImplementedException();
}
