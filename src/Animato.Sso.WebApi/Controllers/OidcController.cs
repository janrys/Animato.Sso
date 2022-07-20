namespace Animato.Sso.WebApi.Controllers;

using System.Security.Claims;
using Animato.Sso.Application.Common;
using Animato.Sso.Application.Common.Interfaces;
using Animato.Sso.Application.Models;
using Animato.Sso.Application.Security;
using Animato.Sso.Domain.Entities;
using Animato.Sso.Domain.Enums;
using Animato.Sso.WebApi.Extensions;
using Animato.Sso.WebApi.Models;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

[Route("")]
public class OidcController : ApiControllerBase
{
    private readonly IDateTimeService dateTime;

    public OidcController(ISender mediator, IDateTimeService dateTime) : base(mediator) => this.dateTime = dateTime;


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
            return RedirectToLoginWithBackRedirect();
        }

        var authorizationResult = await this.CommandForCurrentUser(cancellationToken).User.Authorize(authorizationRequest);

        if (!authorizationResult.IsAuthorized)
        {
            return Forbid();
        }

        if (authorizationResult.AuthorizationFlow == AuthorizationFlowType.Token)
        {
            var fragment = $"access_token={authorizationResult.AccessToken}";

            if (!string.IsNullOrEmpty(authorizationRequest.State))
            {
                fragment += $"&state={authorizationRequest.State}";
            }

            redirectUri.SetFragment(fragment);
            return Redirect(redirectUri.ToString());
        }
        else if (authorizationResult.AuthorizationFlow == AuthorizationFlowType.Code)
        {
            redirectUri.QueryParams.AddOrReplace("code", authorizationResult.Code);

            if (!string.IsNullOrEmpty(authorizationRequest.State))
            {
                redirectUri.QueryParams.AddOrReplace("state", authorizationRequest.State);
            }

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
    /// <param name="cancellationToken">Cancelation token</param>
    /// <returns></returns>
    [HttpGet("login", Name = "Login")]
    public async Task<IActionResult> Login(CancellationToken cancellationToken)
    {
        var fileContents = await System.IO.File.ReadAllTextAsync("./Content/Authorize.html", cancellationToken);
        string userName;

        if (User.Identity.IsAuthenticated)
        {
            var user = await this.QueryForCurrentUser(cancellationToken).User.GetById(User.GetUserId());
            var lastChanged = User.Claims.FirstOrDefault(c => c.Type == "last_changed")?.Value;

            if (!DateTime.TryParse(lastChanged, GlobalOptions.Culture, System.Globalization.DateTimeStyles.AllowWhiteSpaces, out var lastChangedParsed)
                || DateTime.SpecifyKind(lastChangedParsed, DateTimeKind.Utc) < user.LastChanged.AddSeconds(-1))
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return LocalRedirect(Url.GetLocalUrl(Request.Query["from"].ToString() ?? "/login"));
            }

            if (user.AuthorizationMethod != AuthorizationMethod.TotpQrCode)
            {
                return LocalRedirect(Url.GetLocalUrl(Request.Query["from"]));
            }

            userName = user.Login;

            fileContents = RemovePasswordBlock(fileContents);
        }
        else
        {
            userName = "-";
        }
        fileContents = RemoveTotpBlock(fileContents);
        fileContents = fileContents.Replace("***username***", userName);
        fileContents = fileContents.Replace("***redirectUrl***", Request.Query["from"]);

        return new ContentResult
        {
            Content = fileContents,
            ContentType = "text/html"
        };
    }

    private string RemoveTotpBlock(string fileContents)
        => RemoveBlock(fileContents, "***totpblockstart***", "***totpblockend***")
        .Replace("***passwordblockstart***", "")
        .Replace("***passwordblockend***", "");

    private string RemovePasswordBlock(string fileContents)
        => RemoveBlock(fileContents, "***passwordblockstart***", "***passwordblockend***")
        .Replace("***totpblockstart***", "")
        .Replace("***totpblockend***", "");

    private string RemoveBlock(string fileContents, string startAnchor, string endAnchor)
    {
        var start = fileContents.IndexOf(startAnchor, StringComparison.OrdinalIgnoreCase);
        var end = fileContents.IndexOf(endAnchor, StringComparison.OrdinalIgnoreCase) + endAnchor.Length;

        if (start >= 0 && end >= 0)
        {
            return fileContents.Remove(start, end - start);
        }

        return fileContents;
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
        User user = null;
        var authorizationMethod = AuthorizationMethod.Unknown;

        if (loginModel.Action.Equals("logout", StringComparison.OrdinalIgnoreCase))
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }
        else if (loginModel.Action.Equals("validateQrCode", StringComparison.OrdinalIgnoreCase))
        {
            user = await this.QueryForCurrentUser(cancellationToken).User.GetById(User.GetUserId());

            if (user is null)
            {
                return Forbid();
            }

            var result = authenticator.ValidatePin(user.TotpSecretKey, loginModel.QrCode);

            if (!result)
            {
                return Forbid();
            }

            authorizationMethod = AuthorizationMethod.TotpQrCode;
        }
        else
        {
            if (string.IsNullOrEmpty(loginModel.UserName) || string.IsNullOrEmpty(loginModel.Password))
            {
                return Forbid();
            }

            user = await this.Command(cancellationToken).User.Login(loginModel.UserName, loginModel.Password);

            if (user is null)
            {
                return Forbid();
            }

            authorizationMethod = AuthorizationMethod.Password;
        }

        var claims = claimFactory.GenerateClaims(user, authorizationMethod);
        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        var authProperties = new AuthenticationProperties
        {
            AllowRefresh = true,
            ExpiresUtc = dateTime.UtcNowOffset.AddHours(8),
            IsPersistent = true,
            IssuedUtc = dateTime.UtcNowOffset,
            RedirectUri = "\\login"
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);

        if (user.AuthorizationMethod == AuthorizationMethod.TotpQrCode && authorizationMethod == AuthorizationMethod.Password)
        {
            var routeValuesDictionary = new RouteValueDictionary
            {
                { "from", loginModel.RedirectUrl }
            };
            return RedirectToRoute("Login", routeValuesDictionary);
        }

        return LocalRedirect(Url.GetLocalUrl(loginModel.RedirectUrl ?? "/login"));
    }

    /// <summary>
    /// Interactive user information
    /// </summary>
    /// <param name="authenticator"></param>
    /// <param name="cancellationToken">Cancelation token</param>
    /// <returns></returns>
    [HttpGet("login/me", Name = "AboutMe")]
    public async Task<IActionResult> AboutMe([FromServices] IQrCodeTotpAuthenticator authenticator, CancellationToken cancellationToken)
    {
        if (!User.Identity.IsAuthenticated)
        {
            return RedirectToLoginWithBackRedirect();
        }

        var userId = User.GetUserId();

        if (userId == UserId.Empty)
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToLoginWithBackRedirect();
        }

        var user = await this.QueryForCurrentUser(cancellationToken).User.GetById(userId);

        if (user == null)
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToLoginWithBackRedirect();
        }

        var lastChanged = User.Claims.FirstOrDefault(c => c.Type == "last_changed")?.Value;
        if (!DateTime.TryParse(lastChanged, GlobalOptions.Culture, System.Globalization.DateTimeStyles.AllowWhiteSpaces, out var lastChangedParsed)
            || DateTime.SpecifyKind(lastChangedParsed, DateTimeKind.Utc) < user.LastChanged.AddSeconds(-1))
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToLoginWithBackRedirect();
        }

        var fileContents = await System.IO.File.ReadAllTextAsync("./Content/AboutMe.html", cancellationToken);
        fileContents = fileContents.Replace("***username***", user.Login);
        fileContents = fileContents.Replace("***name***", user.Name);
        fileContents = fileContents.Replace("***fullname***", user.FullName);
        fileContents = fileContents.Replace("***id***", user.Id.ToString());
        fileContents = fileContents.Replace("***authorizationType***", user.AuthorizationMethod.Name);
        fileContents = fileContents.Replace("***isBlocked***", user.IsBlocked.ToString());
        fileContents = fileContents.Replace("***isDeleted***", user.IsDeleted.ToString());

        var qrCodeInfo = authenticator.GenerateCode(user.Login, user.TotpSecretKey);
        var qrCodeImageUrl = qrCodeInfo.ImageUrl;
        var manualEntrySetupCode = qrCodeInfo.ManualKey;
        fileContents = fileContents.Replace("***manual2facode***", manualEntrySetupCode);
        fileContents = fileContents.Replace("***qrcodeUrl***", qrCodeImageUrl);

        return new ContentResult
        {
            Content = fileContents,
            ContentType = "text/html"
        };
    }

    private IActionResult RedirectToLoginWithBackRedirect()
    {
        var routeValuesDictionary = new RouteValueDictionary
            {
                { "from", $"{Request.Path}{Request.QueryString}" }
            };
        return RedirectToRoute("Login", routeValuesDictionary);
    }

    /// <summary>
    /// Token endpoint
    /// </summary>
    /// <param name="tokenRequest"></param>
    /// <param name="cancellationToken">Cancelation token</param>
    /// <returns>Token response</returns>
    [HttpPost("tokenjson", Name = "Token")]
    [Consumes("application/json")]
    public Task<IActionResult> TokenJson([FromBody] TokenRequest tokenRequest, CancellationToken cancellationToken)
        => TokenInternal(tokenRequest, cancellationToken);

    /// <summary>
    /// Token endpoint
    /// </summary>
    /// <param name="tokenRequest"></param>
    /// <param name="cancellationToken">Cancelation token</param>
    /// <returns>Token response</returns>
    [HttpPost("token", Name = "TokenForm")]
    [Consumes("application/x-www-form-urlencoded")]
    public Task<IActionResult> TokenForm([FromForm] TokenRequest tokenRequest, CancellationToken cancellationToken)
        => TokenInternal(tokenRequest, cancellationToken);

    /// <summary>
    /// Token endpoint
    /// </summary>
    /// <param name="tokenRequest"></param>
    /// <param name="cancellationToken">Cancelation token</param>
    /// <returns>Token response</returns>
    private async Task<IActionResult> TokenInternal(TokenRequest tokenRequest, CancellationToken cancellationToken)
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
    [HttpPost("tokeninfo", Name = "TokenInfo2")]
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
    /// Get information about authenticated user
    /// </summary>
    /// <param name="accessToken">Access token could be in query parameter or in authorization header</param>
    /// <param name="cancellationToken">Cancelation token</param>
    /// <returns></returns>
    [HttpPost("userinfo", Name = "UserInfoPost")]
    [HttpGet("userinfo", Name = "UserInfo")]
    public async Task<IActionResult> UserInfo([FromQuery(Name = "authorization")] string accessToken, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(accessToken))
        {
            accessToken = Request.Headers.Authorization.FirstOrDefault()?.Replace("bearer ", "", StringComparison.OrdinalIgnoreCase);
        }

        if (string.IsNullOrEmpty(accessToken))
        {
            return BadRequest($"{nameof(accessToken)} must have a value");
        }

        accessToken = accessToken.Replace("bearer ", "", StringComparison.OrdinalIgnoreCase);

        var userInfo = await this.Query(cancellationToken).User.GetUserInfo(accessToken);

        return Ok(userInfo);
    }

    /// <summary>
    /// Logout user and invalidate all tokens
    /// </summary>
    /// <param name="cancellationToken">Cancelation token</param>
    /// <param name="redirectUri">Optional redirect URI after logout</param>
    /// <returns></returns>
    [HttpPost("logout", Name = "LogoutPost")]
    [HttpGet("logout", Name = "LogoutGet")]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken, [FromQuery(Name = "redirect_uri")] string redirectUri = null)
    {
        if (User.Identity.IsAuthenticated)
        {
            await this.CommandForCurrentUser(cancellationToken).Token.RevokeAllTokens();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (string.IsNullOrEmpty(redirectUri))
            {
                return Ok();
            }
            else
            {
                if (Flurl.Url.IsValid(redirectUri))
                {
                    return Redirect(redirectUri);
                }
                else
                {
                    return BadRequest($"Wrong redirect uri {redirectUri}");
                }
            }
        }

        if (string.IsNullOrEmpty(redirectUri) || !Flurl.Url.IsValid(redirectUri))
        {
            return LocalRedirect(Url.GetLocalUrl(Request.Query["from"].ToString() ?? "/login"));
        }
        else
        {
            return Redirect(redirectUri);
        }
    }

    /// <summary>
    /// Get server metadata
    /// </summary>
    /// <param name="metadataService"></param>
    /// <param name="cancellationToken">Cancelation token</param>
    /// <param name="linkGenerator"></param>
    /// <param name="oidcOptions"></param>
    /// <param name="globalOptions"></param>
    /// <returns>Metadata</returns>
    [HttpGet("/.well-known/oauth-authorization-server", Name = "Metadata")]
    [HttpGet("/.well-known/openid-configuration", Name = "MetadataOidc")]
    public async Task<IActionResult> Metadata([FromServices] IMetadataService metadataService
        , [FromServices] LinkGenerator linkGenerator
        , [FromServices] OidcOptions oidcOptions
        , [FromServices] GlobalOptions globalOptions
        , CancellationToken cancellationToken)
    {
        var scopes = await this.Query(cancellationToken).Scope.GetAll();

        cancellationToken.ThrowIfCancellationRequested();
        await Task.CompletedTask;
        var metadata = new SystemMetadata()
        {
            Issuer = metadataService.GetIssuer(),
            AuthorizationEndpoint = linkGenerator.GetUriByAction(HttpContext, "Authorize"),
            TokenEndpoint = linkGenerator.GetUriByAction(HttpContext, "Token"),
            RevocationEndpoint = linkGenerator.GetUriByAction(HttpContext, "Revoke"),
            UserInfoEndpoint = linkGenerator.GetUriByAction(HttpContext, "UserInfo"),
            JsonWebKeySetUri = linkGenerator.GetUriByAction(HttpContext, "JsonWebKeySetMetadata"),
            ResponseTypesSupported = new List<string>(new string[] { "code", "token" }),
            MinimalPasswordLength = oidcOptions.MinimalPasswordLength,
            ScopesSupported = scopes.Select(s => s.Name).ToList(),
            AuthenticationCodeExpiration = oidcOptions.CodeExpirationMinutes * 60,
            CorrelationHeaderName = globalOptions.CorrelationHeaderName

        };
        return Ok(metadata);
    }

    /// <summary>
    /// Get json web key set metadata
    /// </summary>
    /// <param name="certificateManager"></param>
    /// <param name="cancellationToken">Cancelation token</param>
    /// <returns>Json web key set metadata</returns>
    [HttpGet("/.well-known/oauth-jwks", Name = "MetadataJwks")]
    [HttpGet("/.well-known/openid-jwks", Name = "MetadataJwksOidc")]
    public async Task<IActionResult> JsonWebKeySetMetadata([FromServices] ICertificateManager certificateManager
        , CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await Task.CompletedTask;
        var metadata = new JsonWebKeySetMetadata();
        metadata.JsonWebKeys.Add(certificateManager.GetJsonWebKey());
        return Ok(metadata);
    }
}
