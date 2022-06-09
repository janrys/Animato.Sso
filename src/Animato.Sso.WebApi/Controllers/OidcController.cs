namespace Animato.Sso.WebApi.Controllers;

using System.Security.Claims;
using Animato.Sso.Application.Common;
using Animato.Sso.Application.Common.Interfaces;
using Animato.Sso.Application.Models;
using Animato.Sso.Domain.Enums;
using Animato.Sso.WebApi.Extensions;
using Animato.Sso.WebApi.Models;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
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
    /// <returns>Authorization response</returns>
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

        Flurl.Url redirectUri;
        if (!Flurl.Url.IsValid(authorizationRequest.RedirectUri))
        {
            return BadRequest($"{nameof(authorizationRequest.RedirectUri)} is not a valid URI");
        }
        else
        {
            redirectUri = Flurl.Url.Parse(authorizationRequest.RedirectUri);
        }

        if (!User.Identity.IsAuthenticated)
        {
            var routeValuesDictionary = new RouteValueDictionary
            {
                { "from", $"{Request.Path}{Request.QueryString}" }
            };
            return RedirectToRoute("Login", routeValuesDictionary);
        }

        var authorizationResult = await this.CommandForCurrentUser(cancellationToken).User.Authorize(authorizationRequest);

        if (!authorizationResult.IsAuthorized)
        {
            return Forbid();
        }

        if (authorizationResult.AuthorizationFlow == AuthorizationFlowType.Token)
        {
            redirectUri.SetFragment($"access_token={authorizationResult.AccessToken}");
            return Redirect(redirectUri.ToString());
        }
        else if (authorizationResult.AuthorizationFlow == AuthorizationFlowType.Code)
        {
            redirectUri.QueryParams.AddOrReplace("code", authorizationResult.Code);
            return Redirect(redirectUri.ToString());
        }
        else
        {
            return StatusCode(StatusCodes.Status501NotImplemented, "Flow not implemented");
        }
    }


    /// <summary>
    /// Interactive user login
    /// </summary>
    /// <param name="authenticator"></param>
    /// <param name="cancellationToken">Cancelation token</param>
    /// <returns></returns>
    [HttpGet("login", Name = "Login")]
    public async Task<IActionResult> Login([FromServices] IQrCodeTotpAuthenticator authenticator, CancellationToken cancellationToken)
    {

        logger.LogDebug("Executing action {Action}", nameof(Login));
        var fileContents = await System.IO.File.ReadAllTextAsync("./Content/Authorize.html", cancellationToken);


        if (User.Identity.IsAuthenticated)
        {
            var lastChanged = User.Claims.FirstOrDefault(c => c.Type == "last_changed")?.Value;

            if (!DateTime.TryParse(lastChanged, DefaultOptions.Culture, System.Globalization.DateTimeStyles.AllowWhiteSpaces, out var lastChangedParsed)
                || lastChangedParsed <= DateTime.UtcNow.AddMinutes(-1))
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return LocalRedirect(Url.GetLocalUrl(Request.Query["from"].ToString() ?? "/login"));
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
        fileContents = fileContents.Replace("***redirectUrl***", Request.Query["from"]);

        return new ContentResult
        {
            Content = fileContents,
            ContentType = "text/html"
        };
    }

    /// <summary>
    /// User login validation
    /// </summary>
    /// <param name="loginModel"></param>
    /// <param name="authenticator"></param>
    /// <param name="claimFactory"></param>
    /// <param name="cancellationToken">Cancelation token</param>
    /// <returns></returns>
    [HttpPost("authorize/interactive", Name = "AuthorizeInteractive")]
    public async Task<IActionResult> AuthorizeInteractive([FromForm] LoginModel loginModel, [FromServices] IQrCodeTotpAuthenticator authenticator, [FromServices] IClaimFactory claimFactory, CancellationToken cancellationToken)
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
            if (string.IsNullOrEmpty(loginModel.UserName) || string.IsNullOrEmpty(loginModel.Password))
            {
                return Forbid();
            }

            var user = await this.Command(cancellationToken).User.Login(loginModel.UserName, loginModel.Password);

            if (user is null)
            {
                return Forbid();
            }

            var claims = claimFactory.GenerateClaims(user);
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

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

        return LocalRedirect(Url.GetLocalUrl(loginModel.RedirectUrl ?? "/login"));
    }

    /// <summary>
    /// Token endpoint
    /// </summary>
    /// <param name="tokenRequest"></param>
    /// <param name="cancellationToken">Cancelation token</param>
    /// <returns>Token response</returns>
    [HttpPost("token", Name = "Token")]
    public async Task<IActionResult> Token([FromBody] TokenRequest tokenRequest, CancellationToken cancellationToken)
    {
        if (tokenRequest is null)
        {
            return BadRequest($"{nameof(tokenRequest)} must have a value");
        }

        if (string.IsNullOrEmpty(tokenRequest.GrantType))
        {
            return BadRequest($"{nameof(tokenRequest.GrantType)} must have a value");
        }

        if (string.IsNullOrEmpty(tokenRequest.ClientId))
        {
            return BadRequest($"{nameof(tokenRequest.ClientId)} must have a value");
        }

        if (string.IsNullOrEmpty(tokenRequest.ClientSecret))
        {
            return BadRequest($"{nameof(tokenRequest.ClientSecret)} must have a value");
        }

        if (tokenRequest.GrantType.Equals(GrantType.Code.GrantCode, StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrEmpty(tokenRequest.RedirectUri))
            {
                return BadRequest($"{nameof(tokenRequest.RedirectUri)} must have a value");
            }

            if (!Flurl.Url.IsValid(tokenRequest.RedirectUri))
            {
                return BadRequest($"{nameof(tokenRequest.RedirectUri)} is not a valid URI");
            }
        }

        var tokenResult = await this.Command(cancellationToken).User.GetToken(tokenRequest);

        if (!tokenResult.IsAuthorized)
        {
            return Forbid();
        }

        if (tokenResult.GrantType == GrantType.Code || tokenResult.GrantType == GrantType.Refresh)
        {
            return Ok(new TokenResponse(tokenResult));
        }
        else
        {
            return StatusCode(StatusCodes.Status501NotImplemented, $"Grant type {tokenResult.GrantType.Name} not implemented");
        }
    }

    /// <summary>
    /// Revoke token endpoint
    /// </summary>
    /// <param name="revokeRequest"></param>
    /// <param name="cancellationToken">Cancelation token</param>
    /// <returns></returns>
    [HttpPost("revoke", Name = "Revoke")]
    public async Task<IActionResult> Revoke([FromBody] RevokeRequest revokeRequest, CancellationToken cancellationToken)
    {
        if (revokeRequest is null)
        {
            return BadRequest($"{nameof(revokeRequest)} must have a value");
        }

        if (string.IsNullOrEmpty(revokeRequest.Token))
        {
            return BadRequest($"{nameof(revokeRequest.Token)} must have a value");
        }

        await this.Command(cancellationToken).Token.RevokeToken(revokeRequest.Token);

        return Ok();
    }

    /// <summary>
    /// Get token information and status
    /// </summary>
    /// <param name="tokenInfoRequest"></param>
    /// <param name="cancellationToken">Cancelation token</param>
    /// <returns></returns>
    [HttpPost("token-info", Name = "TokenInfo")]
    public async Task<IActionResult> TokenInfo([FromBody] RevokeRequest tokenInfoRequest, CancellationToken cancellationToken)
    {
        if (tokenInfoRequest is null)
        {
            return BadRequest($"{nameof(tokenInfoRequest)} must have a value");
        }

        if (string.IsNullOrEmpty(tokenInfoRequest.Token))
        {
            return BadRequest($"{nameof(tokenInfoRequest.Token)} must have a value");
        }

        var tokenInfo = await this.Query(cancellationToken).Token.GetTokenInfo(tokenInfoRequest.Token);

        return Ok(tokenInfo);
    }

    /// <summary>
    /// Logout user and invalidate all tokens
    /// </summary>
    /// <param name="cancellationToken">Cancelation token</param>
    /// <returns></returns>
    [HttpPost("logout", Name = "Logout")]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        if (User.Identity.IsAuthenticated)
        {
            await this.CommandForCurrentUser(cancellationToken).Token.RevokeAllTokens();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok();
        }

        return LocalRedirect(Url.GetLocalUrl(Request.Query["from"].ToString() ?? "/login"));
    }

    /// <summary>
    /// Get server metadata
    /// </summary>
    /// <param name="metadataService"></param>
    /// <param name="cancellationToken">Cancelation token</param>
    /// <param name="linkGenerator"></param>
    /// <returns>Metadata</returns>
    [HttpGet("/.well-known/oauth-authorization-server", Name = "Metadata")]
    [HttpGet("/.well-known/openid-configuration", Name = "MetadataOidc")]
    public async Task<IActionResult> Metadata([FromServices] IMetadataService metadataService, [FromServices] LinkGenerator linkGenerator, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await Task.CompletedTask;
        var metadata = new SystemMetadata()
        {
            Issuer = metadataService.GetIssuer(),
            AuthorizationEndpoint = linkGenerator.GetUriByAction(HttpContext, "Authorize"),
            TokenEndpoint = linkGenerator.GetUriByAction(HttpContext, "Token"),
            RevocationEndpoint = linkGenerator.GetUriByAction(HttpContext, "Revoke"),
            ResponseTypesSupported = new List<string>(new string[] { "code", "token" }),
        };
        return Ok(metadata);
    }
}
