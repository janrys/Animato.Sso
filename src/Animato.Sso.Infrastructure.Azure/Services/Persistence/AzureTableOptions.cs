namespace Animato.Sso.Infrastructure.AzureStorage.Services.Persistence;

public class AzureTableStorageOptions
{
    public const string ConfigurationKey = nameof(AzureTableStorageOptions);
    public const string ArraySplitter = ",";

    public string ConnectionString { get; set; }
    public string UsersTable { get; set; } = "users";
    public string ApplicationsTable { get; set; } = "applications";
    public string ApplicationRolesTable { get; set; } = "applicationroles";
    public string AuthorizationCodesTable { get; set; } = "authorizationcodes";
    public string TokensTable { get; set; } = "tokens";
    public string UserApplicationRolesTable { get; set; } = "userapplicationroles";
    public string ScopesTable { get; set; } = "scopes";
    public string ClaimsTable { get; set; } = "claims";
    public string ClaimScopesTable { get; set; } = "claimscopes";
    public string UserClaimsTable { get; set; } = "userclaims";
    public string ApplicationScopesTable { get; set; } = "applicationscopes";
}
