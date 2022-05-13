namespace Animato.Sso.Infrastructure.Services.Persistence;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Animato.Sso.Application.Common.Interfaces;
using Animato.Sso.Application.Exceptions;
using Animato.Sso.Application.Models;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;

public class AzureBlobFileStorageService : IFileStorageService
{
    private const string METADATA_ID = "Id";
    private const string METADATA_NAME = "Name";
    private const string METADATA_CONTENT_TYPE = "ContentType";
    private readonly AzureBlobStorageOptions options;
    private readonly ILogger<AzureBlobFileStorageService> logger;
    private readonly BlobServiceClient blobServiceClient;
    private BlobContainerClient assetContainerClient;

    public AzureBlobFileStorageService(AzureBlobStorageOptions options, ILogger<AzureBlobFileStorageService> logger)
    {
        this.options = options ?? throw new ArgumentNullException(nameof(options));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        blobServiceClient = new BlobServiceClient(this.options.ConnectionString);
    }

    public async Task<AssetLocation> Save(Stream stream, Guid id, string name, string contentType, CancellationToken cancellationToken)
    {
        if (!await EnsureContainerExists(cancellationToken))
        {
            logger.LogError("Blob container {ContainerName} does not exist", options.AssetContainer);
            throw new DataAccessException($"Container {options.AssetContainer} does not exist");
        }

        var assetLocation = new AssetLocation
        {
            Path = $"{id}/{name}"
        };
        var blobClient = assetContainerClient.GetBlobClient(assetLocation.Path);
        assetLocation.Url = blobClient.Uri.ToString();

        try
        {
            await blobClient.UploadAsync(stream, cancellationToken);

            await blobClient.SetMetadataAsync(CreateMetadata(id, name, contentType), cancellationToken: cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error uploading asset {FileName}", assetLocation.Path);
            throw new DataAccessException($"Error uploading asset {assetLocation.Path}");
        }

        return assetLocation;
    }

    private static IDictionary<string, string> CreateMetadata(Guid id, string name, string contentType) => new Dictionary<string, string>
        {
            { METADATA_ID, id.ToString() },
            { METADATA_NAME, name },
            { METADATA_CONTENT_TYPE, contentType }
        };

    private async Task<bool> EnsureContainerExists(CancellationToken cancellationToken)
    {
        if (assetContainerClient is not null)
        {
            return true;
        }

        assetContainerClient = blobServiceClient.GetBlobContainerClient(options.AssetContainer);

        try
        {
            await assetContainerClient.CreateIfNotExistsAsync(PublicAccessType.Blob
                , cancellationToken: cancellationToken);
            return true;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error creating blob container {ContainerName}", options.AssetContainer);
            return false;
        }
    }

    public async Task Delete(Guid id, CancellationToken cancellationToken)
    {
        if (!await EnsureContainerExists(cancellationToken))
        {
            logger.LogError("Blob container {ContainerName} does not exist", options.AssetContainer);
            return;
        }

        var blobs = new List<BlobItem>();
        var blobResult = assetContainerClient.GetBlobsAsync(prefix: id.ToString(), cancellationToken: cancellationToken);
        await foreach (var pages in blobResult.AsPages())
        {
            blobs.AddRange(pages.Values);
        }

        if (blobs.Any())
        {
            var deleteTasks = blobs.Select(b => assetContainerClient.DeleteBlobIfExistsAsync(b.Name, cancellationToken: cancellationToken));
            await Task.WhenAll(deleteTasks);
        }
    }

    public Task Seed() => Task.CompletedTask;
}
