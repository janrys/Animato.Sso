namespace Animato.Sso.Infrastructure.Services.Persistence;

using System.Collections.Generic;
using Animato.Sso.Application.Common.Logging;
using Animato.Sso.Domain.Entities;
using Microsoft.Extensions.Logging;

public class InMemoryDataContext
{
    private readonly ILogger<InMemoryDataContext> logger;

    public InMemoryDataContext(ILogger<InMemoryDataContext> logger)
    {
        this.logger = logger;
        this.logger.PersistenceLayerLoadingInformation("in-memory");
    }

    public List<User> Users { get; set; } = new List<User>();
    public List<Application> Applications { get; set; } = new List<Application> { };
    public List<ApplicationRole> ApplicationRoles { get; set; } = new List<ApplicationRole> { };
    public List<UserApplicationRole> UserApplicationRoles { get; set; } = new List<UserApplicationRole> { };
    public List<ApplicationScope> ApplicationScopes { get; set; } = new List<ApplicationScope> { };
    public List<AuthorizationCode> Codes { get; set; } = new List<AuthorizationCode> { };
    public List<Token> Tokens { get; set; } = new List<Token> { };
    public List<Scope> Scopes { get; set; } = new List<Scope> { };
    public List<Claim> Claims { get; set; } = new List<Claim> { };
    public List<ClaimScope> ClaimScopes { get; set; } = new List<ClaimScope> { };
    public List<UserClaim> UserClaims { get; set; } = new List<UserClaim> { };
}
