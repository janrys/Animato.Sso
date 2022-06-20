namespace Animato.Sso.Domain.Entities;

public class Claim
{
    public ClaimId Id { get; set; }
    public string Name { get; set; }

    public static readonly Claim Mail = new() { Id = new ClaimId(new Guid("88248AE1-079F-CCCC-CCCC-182503C14578")), Name = nameof(Mail) };
    public static readonly Claim Phone = new() { Id = new ClaimId(new Guid("E88415E3-EE79-DDDD-AAAA-40AF1E7DBDA7")), Name = nameof(Phone) };
    public static readonly Claim Gender = new() { Id = new ClaimId(new Guid("E88415E3-EE79-BBBB-BBBB-40AF1E7DBDA7")), Name = nameof(Gender) };
}

