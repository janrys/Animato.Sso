namespace Animato.Sso.WebApi.Services;

using Animato.Sso.Application.Common;
using Animato.Sso.Application.Common.Interfaces;
using Microsoft.AspNetCore.Hosting.Server;

public class MetadataService : IMetadataService
{
    private readonly IServer server;
    private readonly OidcOptions oidcOptions;

    public MetadataService(IServer server, OidcOptions oidcOptions)
    {
        this.server = server ?? throw new ArgumentNullException(nameof(server));
        this.oidcOptions = oidcOptions ?? throw new ArgumentNullException(nameof(oidcOptions));
    }

    public string GetIssuer() => server.Features
            .Get<Microsoft.AspNetCore.Hosting.Server.Features.IServerAddressesFeature>()
            .Addresses.FirstOrDefault() ?? oidcOptions.Issuer;
}
