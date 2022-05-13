namespace Animato.Sso.Application.Security;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using Animato.Sso.Application.Common.Interfaces;

public class AttributeAuthorizationService : IAuthorizationService
{
    public Task<bool> IsAllowed<TRequest>(TRequest request, ClaimsPrincipal user) where TRequest : notnull
    {
        // allow all, if user is empty
        if (user == null)
        {
            return Task.FromResult(true);
        }

        var authorizeAttributes = request.GetType().GetCustomAttributes<AuthorizeAttribute>();

        if (!authorizeAttributes.Any())
        {
            return Task.FromResult(true);
        }

        if (IsInRole(authorizeAttributes, user))
        {
            return Task.FromResult(true);
        }

        if (IsInPolicy(authorizeAttributes, user))
        {
            return Task.FromResult(true);
        }

        return Task.FromResult(false);
    }

    private static bool IsInPolicy(IEnumerable<AuthorizeAttribute> authorizeAttributes, ClaimsPrincipal user)
    {
        var authorizeAttributesWithPolicies = authorizeAttributes.Where(a => !string.IsNullOrWhiteSpace(a.Policy));

        if (authorizeAttributesWithPolicies.Any())
        {
            foreach (var policy in authorizeAttributesWithPolicies.Select(a => a.Policy))
            {
                ; // implement policy check
            }
        }

        return false;
    }

    private static bool IsInRole(IEnumerable<AuthorizeAttribute> authorizeAttributes, ClaimsPrincipal user)
    {
        var authorizeAttributesWithRoles = authorizeAttributes.Where(a => !string.IsNullOrWhiteSpace(a.Roles));

        if (authorizeAttributesWithRoles.Any())
        {
            foreach (var roles in authorizeAttributesWithRoles.Select(a => a.Roles.Split(',')))
            {
                foreach (var role in roles)
                {
                    if (user.IsInRole(role.Trim()))
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }
}
