namespace Animato.Sso.Domain.Entities;

public class UserClaim
{
    public UserId UserId { get; set; }
    public ClaimId ClaimId { get; set; }
    public string Value { get; set; }
}

