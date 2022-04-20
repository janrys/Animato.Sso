namespace Animato.Sso.WebApi.Controllers;

using System.Security.Claims;
using Animato.Sso.WebApi.Extensions;
using Animato.Sso.WebApi.Models;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

[Route("")]
public class OidcController : ApiControllerBase
{
    private readonly ILogger<OidcController> logger;

    public OidcController(ISender mediator, ILogger<OidcController> logger) : base(mediator) => this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>
    /// Authorization endpoint
    /// </summary>
    /// <param name="cancellationToken">Cancelation token</param>
    /// <returns>Asset  metadata</returns>
    [HttpGet("authorize", Name = "Authorize")]
    public async Task<ContentResult> Authorize(CancellationToken cancellationToken)
    {
        logger.LogDebug("Executing action {Action}", nameof(Authorize));
        var fileContents = await System.IO.File.ReadAllTextAsync("./Content/Authorize.html", cancellationToken);

        string userName;
        if (User.Identity.IsAuthenticated)
        {
            userName = User.Identity.Name;
        }
        else
        {
            userName = "not authenticated";
        }
        fileContents = fileContents.Replace("***username***", userName);

        return new ContentResult
        {
            Content = fileContents,
            ContentType = "text/html"
        };
    }

    /// <summary>
    /// Authorization endpoint for users
    /// </summary>
    /// <param name="loginModel"></param>
    /// <param name="cancellationToken">Cancelation token</param>
    /// <returns>Asset  metadata</returns>
    [HttpPost("authorize/interactive", Name = "AuthorizeInteractive")]
    public async Task<IActionResult> AuthorizeInteractive([FromForm] LoginModel loginModel, CancellationToken cancellationToken)
    {
        logger.LogDebug("Executing action {Action}", nameof(AuthorizeInteractive));


        if (loginModel.Action.Equals("logout", StringComparison.OrdinalIgnoreCase))
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }
        else
        {
            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, loginModel.UserName),
            new Claim("FullName", loginModel.UserName),
            new Claim(ClaimTypes.Role, "Administrator"),
        };

            var claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                AllowRefresh = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8),
                IsPersistent = true,
                IssuedUtc = DateTimeOffset.UtcNow,
                RedirectUri = "\authorize"
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);
        }

        return LocalRedirect(Url.GetLocalUrl("/authorize"));
    }
}
