namespace Animato.Sso.Application.Security;
using System.Security.Claims;
using Animato.Sso.Application.Common.Interfaces;

public class StaticMapAuthorizationService : IAuthorizationService
{
    public Task<bool> IsAllowed<TRequest>(TRequest request, ClaimsPrincipal user) where TRequest : notnull => Task.FromResult(true);
}

public class AllowAllAuthorizationService : IAuthorizationService
{
    public Task<bool> IsAllowed<TRequest>(TRequest request, ClaimsPrincipal user) where TRequest : notnull => Task.FromResult(true);
}
