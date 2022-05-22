namespace Animato.Sso.WebApi.Controllers;

using System.Security.Claims;
using Animato.Sso.Application.Common;
using Animato.Sso.Application.Common.Interfaces;
using Animato.Sso.Application.Models;
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
    /// <param name="authorizationRequest"></param>
    /// <param name="cancellationToken">Cancelation token</param>
    /// <returns>Asset  metadata</returns>
    [HttpGet("authorize", Name = "Authorize")]
    public async Task<IActionResult> Authorize([FromQuery] AuthorizationRequest authorizationRequest, CancellationToken cancellationToken)
    {
        if (authorizationRequest is null)
        {
            return BadRequest($"{nameof(authorizationRequest)} must have a value");
        }

        if (string.IsNullOrEmpty(authorizationRequest.ResponseType))
        {
            return BadRequest($"{nameof(authorizationRequest.ResponseType)} must have a value");
        }

        if (string.IsNullOrEmpty(authorizationRequest.ClientId))
        {
            return BadRequest($"{nameof(authorizationRequest.ClientId)} must have a value");
        }

        if (string.IsNullOrEmpty(authorizationRequest.RedirectUri))
        {
            return BadRequest($"{nameof(authorizationRequest.RedirectUri)} must have a value");
        }

        if (!User.Identity.IsAuthenticated)
        {
            var routeValuesDictionary = new RouteValueDictionary();
            Request.Query.Keys.ToList().ForEach(key => routeValuesDictionary.Add(key, Request.Query[key]));
            return RedirectToRoute("login", routeValuesDictionary);
        }

        var authorizationResult = await this.CommandForCurrentUser(cancellationToken).User.Authorize(authorizationRequest);

        if (!authorizationResult.IsAuthorized)
        {
            return Forbid();
        }

        return Ok(authorizationRequest);
    }


    /// <summary>
    /// Authorization endpoint
    /// </summary>
    /// <param name="authenticator"></param>
    /// <param name="cancellationToken">Cancelation token</param>
    /// <returns>Asset  metadata</returns>
    [HttpGet("login", Name = "login")]
    public async Task<IActionResult> Login([FromServices] IQrCodeTotpAuthenticator authenticator, CancellationToken cancellationToken)
    {
        logger.LogDebug("Executing action {Action}", nameof(Login));
        var fileContents = await System.IO.File.ReadAllTextAsync("./Content/Authorize.html", cancellationToken);


        if (User.Identity.IsAuthenticated)
        {
            var lastChanged = User.Claims.FirstOrDefault(c => c.Type == "LastChanged")?.Value;

            if (!DateTime.TryParse(lastChanged, DefaultOptions.Culture, System.Globalization.DateTimeStyles.AllowWhiteSpaces, out var lastChangedParsed)
                || lastChangedParsed <= DateTime.UtcNow.AddMinutes(-1))
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return RedirectToRoute("Authorize");
            }
        }

        string userName;
        if (User.Identity.IsAuthenticated)
        {
            userName = User.Identity.Name;

            var qrCodeInfo = authenticator.GenerateCode(userName, "secretkey");
            var qrCodeImageUrl = qrCodeInfo.ImageUrl;
            var manualEntrySetupCode = qrCodeInfo.ManualKey;
            fileContents = fileContents.Replace("***manual2facode***", manualEntrySetupCode);
            fileContents = fileContents.Replace("***qrcodeUrl***", qrCodeImageUrl);
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
    /// <param name="authenticator"></param>
    /// <param name="cancellationToken">Cancelation token</param>
    /// <returns>Asset  metadata</returns>
    [HttpPost("authorize/interactive", Name = "AuthorizeInteractive")]
    public async Task<IActionResult> AuthorizeInteractive([FromForm] LoginModel loginModel, [FromServices] IQrCodeTotpAuthenticator authenticator, CancellationToken cancellationToken)
    {
        logger.LogDebug("Executing action {Action}", nameof(AuthorizeInteractive));


        if (loginModel.Action.Equals("logout", StringComparison.OrdinalIgnoreCase))
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }
        else if (loginModel.Action.Equals("validateQrCode", StringComparison.OrdinalIgnoreCase))
        {
            var result = authenticator.ValidatePin("secretkey", loginModel.QrCode);
        }
        else
        {
            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, loginModel.UserName),
            new Claim(ClaimTypes.Name, loginModel.UserName),
            new Claim("FullName", loginModel.UserName),
            new Claim(ClaimTypes.Role, "Administrator"),
            new Claim("LastChanged", DateTime.UtcNow.ToString(DefaultOptions.DatePattern, DefaultOptions.Culture)) // last change from database
        };

            var claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                AllowRefresh = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8),
                IsPersistent = true,
                IssuedUtc = DateTimeOffset.UtcNow,
                RedirectUri = "\\login"
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);


        }

        // return LocalRedirect(Url.GetLocalUrl("/login"));
        return RedirectToRoute("login");
    }
}
