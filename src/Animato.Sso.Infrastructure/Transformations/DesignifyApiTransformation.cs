namespace Animato.Sso.Infrastructure.Transformations;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Animato.Sso.Application.Common.Interfaces;
using Animato.Sso.Application.Exceptions;
using Microsoft.Extensions.Logging;

public class DesignifyApiTransformation : BaseTransformation, IAssetTransformation
{
    public const string HTTP_CLIENT_NAME = "DesignifyCLient";
    private readonly DesignifyOptions designifyOptions;
    private readonly ILogger<DesignifyApiTransformation> logger;
    private readonly IHttpClientFactory httpClientFactory;

    public DesignifyApiTransformation(DesignifyOptions designifyOptions, ILogger<DesignifyApiTransformation> logger, IHttpClientFactory httpClientFactory) : base("Designify", "Transform image in Designify API. Design ID has to be defined in Designify. Parameters (id={designId})", "*")
    {
        this.designifyOptions = designifyOptions ?? throw new ArgumentNullException(nameof(designifyOptions));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    }
    public override async Task<Stream> Transform(Stream asset, string parameters = null)
    {
        var parsedParameters = ParseParameters(parameters);
        var designIdParameter = parsedParameters?.FirstOrDefault(p => p.Key.Equals("id", StringComparison.OrdinalIgnoreCase)) ?? null;

        if (!designIdParameter.HasValue || string.IsNullOrEmpty(designIdParameter.Value.Value))
        {
            throw new ArgumentNullException(nameof(parameters));
        }

        var designId = designIdParameter.Value.Value;

        using var httpClient = httpClientFactory.CreateClient(HTTP_CLIENT_NAME);
        using var formData = new MultipartFormDataContent();
        var fileName = Guid.NewGuid().ToString();

        var ms = new MemoryStream();
        await asset.CopyToAsync(ms);
        formData.Headers.Add("X-Api-Key", designifyOptions.ApiKey);
        formData.Add(new ByteArrayContent(ms.ToArray()), fileName, fileName);
        var response = await httpClient.PostAsync($"{designifyOptions.Url}:{designId}", formData);

        if (response.IsSuccessStatusCode)
        {
            ms = new MemoryStream();
            await response.Content.CopyToAsync(ms);
            return ms;
        }
        else
        {
            logger.LogError("Error calling Designify API with design id {DesignId}. Response: {Response}", designId, await response.Content.ReadAsStringAsync());
            throw new DataAccessException("Error calling Designify API");
        }
    }
}
