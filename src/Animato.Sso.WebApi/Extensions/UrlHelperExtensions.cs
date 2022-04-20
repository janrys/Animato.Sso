namespace Animato.Sso.WebApi.Extensions;

using Microsoft.AspNetCore.Mvc;

public static class UrlHelperExtensions
{
    public static string GetLocalUrl(this IUrlHelper urlHelper, string localUrl)
    {
        if (!urlHelper.IsLocalUrl(localUrl))
        {
            return urlHelper!.Page("/authorize");
        }

        return localUrl;
    }
}
