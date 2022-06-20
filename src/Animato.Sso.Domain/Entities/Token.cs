namespace Animato.Sso.Domain.Entities;
using System;
using Animato.Sso.Domain.Enums;

public class Token
{
    public TokenId Id { get; set; }
    public TokenType TokenType { get; set; }
    public ApplicationId ApplicationId { get; set; }
    public UserId UserId { get; set; }
    public TokenId? RefreshTokenId { get; set; }
    public string Value { get; set; }
    public DateTime Created { get; set; }
    public DateTime Expiration { get; set; }
    public DateTime? Revoked { get; set; }
}

public static class TokenExtensions
{
    public static bool IsExpired(this Token token, DateTime utcNow) => utcNow > token.Expiration;
}

