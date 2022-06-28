namespace Animato.Sso.Domain.Entities;

public class UserClaim
{
    public UserClaimId Id { get; set; }
    public UserId UserId { get; set; }
    public ClaimId ClaimId { get; set; }
    public string Value { get; set; }
}

