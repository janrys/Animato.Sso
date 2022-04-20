namespace Animato.Sso.Application.Common.Interfaces;
using System.Security.Claims;

public interface IAuthorizationService
{
    Task<bool> IsAllowed<TRequest>(TRequest request, ClaimsPrincipal user) where TRequest : notnull;
}
