namespace Animato.Sso.Infrastructure.Services.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Animato.Sso.Application.Common.Interfaces;
using Animato.Sso.Application.Exceptions;
using Animato.Sso.Application.Models;
using Animato.Sso.Domain.Entities;
using Azure;
using Azure.Data.Tables;
using Microsoft.Extensions.Logging;

public class AzureTableMetadataStorageService : IMetadataStorageService, ITransformationStorageService
{
    private readonly AzureTableStorageOptions options;
    private readonly ILogger<AzureTableMetadataStorageService> logger;
    private readonly TableServiceClient tableServiceClient;
    private const int MAX_PER_PAGE = 100;
    private TableClient assetTableClient;
    private TableClient transformationTableClient;

    public AzureTableMetadataStorageService(AzureTableStorageOptions options, ILogger<AzureTableMetadataStorageService> logger)
    {
        this.options = options ?? throw new ArgumentNullException(nameof(options));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        tableServiceClient = new TableServiceClient(options.ConnectionString);
    }

    public async Task DeleteAsset(Guid id, CancellationToken cancellationToken)
    {
        await ThrowExceptionIfTableNotExists(cancellationToken);

        var asset = await GetAsset(id, cancellationToken);

        if (asset == null)
        {
            return;
        }

        var tableEntity = new AssetMetadataTableEntity(asset.ContentType, asset.Id);

        try
        {
            await assetTableClient.DeleteEntityAsync(tableEntity.PartitionKey, tableEntity.RowKey, cancellationToken: cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error deleting asset");
            throw new DataAccessException("Error deleting asset", exception);
        }

        return;
    }

    public async Task<AssetMetadata> GetAsset(Guid id, CancellationToken cancellationToken)
    {
        await ThrowExceptionIfTableNotExists(cancellationToken);

        try
        {
            var queryResult = assetTableClient.QueryAsync<AssetMetadataTableEntity>(a => a.RowKey == id.ToString(), cancellationToken: cancellationToken);
            var results = new List<AssetMetadataTableEntity>();

            await foreach (var page in queryResult.AsPages())
            {
                results.AddRange(page.Values);
            }

            if (results.Count == 1)
            {
                return results.First().ToAssetMetadata();
            }

            if (results.Count == 0)
            {
                return null;
            }

            throw new DataAccessException($"Found duplicate assets ({results.Count}) for id {id}");
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error loading asset metadata");
            throw new DataAccessException("Error loading asset metadata", exception);
        }
    }

    public async Task<IEnumerable<AssetMetadata>> GetAssets(CancellationToken cancellationToken)
    {
        await ThrowExceptionIfTableNotExists(cancellationToken);

        try
        {
            var queryResult = assetTableClient.QueryAsync<AssetMetadataTableEntity>(cancellationToken: cancellationToken, maxPerPage: MAX_PER_PAGE);
            var results = new List<AssetMetadataTableEntity>();

            await foreach (var page in queryResult.AsPages())
            {
                results.AddRange(page.Values);
            }

            return results.Select(e => e.ToAssetMetadata());
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error loading asset metadata");
            throw new DataAccessException("Error loading asset metadata", exception);
        }
    }


    public async Task<AssetMetadata> InsertAsset(AssetMetadata asset, CancellationToken cancellationToken)
    {
        await ThrowExceptionIfTableNotExists(cancellationToken);

        try
        {
            var assetEntity = new AssetMetadataTableEntity(asset.ContentType, asset.Id, asset.Variants);
            await assetTableClient.AddEntityAsync(assetEntity, cancellationToken);
            return asset;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error inserting asset metadata");
            throw new DataAccessException("Error inserting asset metadata", exception);
        }
    }

    public Task<AssetMetadata> UpdateAsset(AssetMetadata asset, CancellationToken cancellationToken) => throw new NotImplementedException();
    public async Task Seed()
    {
        await SeedAssets();
        await SeedTransformations();
    }

    private Task SeedTransformations() => Task.CompletedTask;
    private Task SeedAssets() => Task.CompletedTask;

    private async Task<bool> ThrowExceptionIfTableNotExists(CancellationToken cancellationToken)
    {
        if (assetTableClient is not null && transformationTableClient is not null)
        {
            return true;
        }

        assetTableClient = tableServiceClient.GetTableClient(options.AssetTable);

        try
        {
            await assetTableClient.CreateIfNotExistsAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error creating table {TableName}", options.AssetTable);
            throw new DataAccessException($"Table {options.AssetTable} does not exist");
        }

        transformationTableClient = tableServiceClient.GetTableClient(options.TransformationTable);

        try
        {
            await transformationTableClient.CreateIfNotExistsAsync(cancellationToken);
            return true;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error creating table {TableName}", options.TransformationTable);
            throw new DataAccessException($"Table {options.TransformationTable} does not exist");
        }
    }

    public async Task<IEnumerable<TransformationDefinition>> GetTransformations(CancellationToken cancellationToken)
    {
        await ThrowExceptionIfTableNotExists(cancellationToken);

        try
        {
            var queryResult = transformationTableClient.QueryAsync<TransformationDefinitionTableEntity>(cancellationToken: cancellationToken, maxPerPage: MAX_PER_PAGE);
            var results = new List<TransformationDefinitionTableEntity>();

            await foreach (var page in queryResult.AsPages())
            {
                results.AddRange(page.Values);
            }

            return results.Select(e => e.ToTransformationDefinition());
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error loading transformation");
            throw new DataAccessException("Error loading transformation", exception);
        }
    }
    public async Task<TransformationDefinition> GetTransformation(Guid id, CancellationToken cancellationToken)
    {
        await ThrowExceptionIfTableNotExists(cancellationToken);

        try
        {
            var queryResult = transformationTableClient.QueryAsync<TransformationDefinitionTableEntity>(a => a.RowKey == id.ToString(), cancellationToken: cancellationToken);
            var results = new List<TransformationDefinitionTableEntity>();

            await foreach (var page in queryResult.AsPages())
            {
                results.AddRange(page.Values);
            }

            if (results.Count == 1)
            {
                return results.First().ToTransformationDefinition();
            }

            if (results.Count == 0)
            {
                return null;
            }

            throw new DataAccessException($"Found duplicate transformations ({results.Count}) for id {id}");
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error loading transformation");
            throw new DataAccessException("Error loading transformation", exception);
        }
    }
    public async Task<TransformationDefinition> InsertTransformation(TransformationDefinition transformation, CancellationToken cancellationToken)
    {
        await ThrowExceptionIfTableNotExists(cancellationToken);

        try
        {
            var transformationEntity = new TransformationDefinitionTableEntity(transformation.Id)
            {
                Definition = transformation.Definition,
                Description = transformation.Description
            };
            await transformationTableClient.AddEntityAsync(transformationEntity, cancellationToken);
            return transformation;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error inserting transformation");
            throw new DataAccessException("Error inserting transformation", exception);
        }
    }

    public Task<TransformationDefinition> UpdateTransformation(TransformationDefinition transformation, CancellationToken cancellationToken) => throw new NotImplementedException();
    public async Task DeleteTransformation(Guid id, CancellationToken cancellationToken)
    {
        await ThrowExceptionIfTableNotExists(cancellationToken);

        var transformation = await GetTransformation(id, cancellationToken);

        if (transformation == null)
        {
            return;
        }

        var tableEntity = new TransformationDefinitionTableEntity(transformation.Id);

        try
        {
            await transformationTableClient.DeleteEntityAsync(tableEntity.PartitionKey, tableEntity.RowKey, cancellationToken: cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error deleting transformation");
            throw new DataAccessException("Error deleting transformation", exception);
        }

        return;
    }
}
