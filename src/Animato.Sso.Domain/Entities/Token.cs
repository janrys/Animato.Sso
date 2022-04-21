namespace Animato.Sso.Domain.Entities;
using System;
using Animato.Sso.Domain.Enums;

public class Token
{
    public TokenId Id { get; set; }
    public TokenType TokenType { get; set; }
    public ApplicationId Applicationid { get; set; }
    public UserId UserId { get; set; }
    public string Value { get; set; }
    public DateTime Created { get; set; }
    public bool IsValid { get; set; }
    public DateTime? Expired { get; set; }
    public DateTime? Revoked { get; set; }
}

