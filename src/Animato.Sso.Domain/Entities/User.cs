namespace Animato.Sso.Domain.Entities;
using System;
using Animato.Sso.Domain.Enums;

public class User
{
    public UserId Id { get; set; }
    public string Name { get; set; }
    public string Login { get; set; }
    public string Salt { get; set; }
    public string Password { get; set; }
}

public class Application
{
    public ApplicationId Id { get; set; }
    public string Name { get; set; }

}

public class Claim
{
    public ClaimId Id { get; set; }
    public string Name { get; set; }
}

public class Scope
{
    public ScopeId Id { get; set; }
    public string Name { get; set; }

    public static readonly Scope General = new() { Id = new ScopeId(new Guid("06B61D08-A21F-4EEB-862A-0E565840D138")), Name = "General" };
    public static readonly Scope Mail = new() { Id = new ScopeId(new Guid("88248AE1-079F-4BFF-9CF6-182503C14578")), Name = "Mail" };
    public static readonly Scope Online = new() { Id = new ScopeId(new Guid("C4A1B2FC-D031-449C-B25C-BDDA6848F798")), Name = "Online" };
    public static readonly Scope Phone = new() { Id = new ScopeId(new Guid("E88415E3-EE79-43CD-A163-40AF1E7DBDA7")), Name = "Phone" };
    public static readonly Scope Role = new() { Id = new ScopeId(new Guid("A2DBD865-7A90-44FA-B2F3-B43DD3D11199")), Name = "Role" };
}

public class UserClaim
{
    public UserId UserId { get; set; }
    public ClaimId ClaimId { get; set; }
    public string Value { get; set; }
}

public class ApplicationRole
{
    public ApplicationRoleId Id { get; set; }
    public ApplicationId ApplicationId { get; set; }
    public string Name { get; set; }
}

public class UserApplicationRole
{
    public UserId UserId { get; set; }
    public ApplicationRoleId ApplicationRoleId { get; set; }
}


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

public class UserId : GuidEntityId
{
    public UserId(Guid value) : base(value)
    {
    }

    public static UserId New() => new(Guid.NewGuid());
    public static UserId Empty { get; } = new(Guid.Empty);
}

public class ApplicationId : GuidEntityId
{
    public ApplicationId(Guid value) : base(value)
    {
    }

    public static ApplicationId New() => new(Guid.NewGuid());
    public static ApplicationId Empty { get; } = new(Guid.Empty);
}

public class ScopeId : GuidEntityId
{
    public ScopeId(Guid value) : base(value)
    {
    }

    public static ScopeId New() => new(Guid.NewGuid());
    public static ScopeId Empty { get; } = new(Guid.Empty);
}

public class ApplicationRoleId : GuidEntityId
{
    public ApplicationRoleId(Guid value) : base(value)
    {
    }

    public static ApplicationRoleId New() => new(Guid.NewGuid());
    public static ApplicationRoleId Empty { get; } = new(Guid.Empty);
}

public class ClaimId : GuidEntityId
{
    public ClaimId(Guid value) : base(value)
    {
    }

    public static ClaimId New() => new(Guid.NewGuid());
    public static ClaimId Empty { get; } = new(Guid.Empty);
}

public class TokenId : GuidEntityId
{
    public TokenId(Guid value) : base(value)
    {
    }

    public static TokenId New() => new(Guid.NewGuid());
    public static TokenId Empty { get; } = new(Guid.Empty);
}


public abstract class EntityId<T> : IComparable<EntityId<T>>, IEquatable<EntityId<T>> where T : IComparable
{
    public EntityId(T value) => Value = value;

    public T Value { get; }

    public bool Equals(EntityId<T> other) => Value.Equals(other.Value);
    public int CompareTo(EntityId<T> other) => Value.CompareTo(other.Value);

    public override bool Equals(object obj)
    {
        if (obj is null)
        {
            return false;
        }

        return obj is EntityId<T> other && Equals(other);
    }

    public override int GetHashCode() => Value.GetHashCode();

    public override string ToString() => Value.ToString();
    public static bool operator ==(EntityId<T> a, EntityId<T> b) => a.CompareTo(b) == 0;
    public static bool operator !=(EntityId<T> a, EntityId<T> b) => !(a == b);
}

public abstract class GuidEntityId : EntityId<Guid>
{
    protected GuidEntityId(Guid value) : base(value)
    {
    }
}
