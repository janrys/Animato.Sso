namespace Animato.Sso.Infrastructure.AzureStorage.Services.Persistence;

using System;
using System.Threading;
using System.Threading.Tasks;
using Animato.Sso.Application.Exceptions;
using Azure;
using Azure.Data.Tables;
using Microsoft.Extensions.Logging;

/// <summary>
/// https://docs.microsoft.com/en-us/rest/api/storageservices/understanding-the-table-service-data-model
/// </summary>
public class AzureTableStorageDataContext
{
    private readonly AzureTableStorageOptions options;
    private readonly ILogger<AzureTableStorageDataContext> logger;
    private readonly TableServiceClient tableServiceClient;
    public const int MAX_PER_PAGE = 100;
    private bool wasTableCheckOk = false;

    public TableClient Users { get; private set; }
    public TableClient Applications { get; private set; }
    public TableClient ApplicationRoles { get; private set; }
    public TableClient AuthorizationCodes { get; private set; }
    public TableClient Tokens { get; private set; }
    public TableClient UserApplicationRoles { get; private set; }

    public AzureTableStorageDataContext(AzureTableStorageOptions options, ILogger<AzureTableStorageDataContext> logger)
    {
        this.options = options ?? throw new ArgumentNullException(nameof(options));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        tableServiceClient = new TableServiceClient(options.ConnectionString);
    }

    public async Task<bool> ThrowExceptionIfTableNotExists(CancellationToken cancellationToken)
    {
        if (wasTableCheckOk)
        {
            return wasTableCheckOk;
        }

        Users = await EnsureTableExists(options.UsersTable, cancellationToken);
        Applications = await EnsureTableExists(options.ApplicationsTable, cancellationToken);
        ApplicationRoles = await EnsureTableExists(options.ApplicationRolesTable, cancellationToken);
        AuthorizationCodes = await EnsureTableExists(options.AuthorizationCodesTable, cancellationToken);
        Tokens = await EnsureTableExists(options.TokensTable, cancellationToken);
        UserApplicationRoles = await EnsureTableExists(options.UserApplicationRolesTable, cancellationToken);

        wasTableCheckOk = true;
        return wasTableCheckOk;
    }

    private async Task<TableClient> EnsureTableExists(string tableName, CancellationToken cancellationToken)
    {
        var tableClient = tableServiceClient.GetTableClient(tableName);

        try
        {
            await tableClient.CreateIfNotExistsAsync(cancellationToken);
            return tableClient;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error creating table {TableName}", tableName);
            throw new DataAccessException($"Table {tableName} does not exist");
        }
    }

    /// <summary>
    /// Deletes all rows from the table
    /// </summary>
    /// <param name="tableClient">The authenticated TableClient</param>
    /// <returns></returns>
    public static async Task DeleteAllEntitiesAsync(TableClient tableClient, CancellationToken cancellationToken)
    {
        // Only the PartitionKey & RowKey fields are required for deletion
        var entities = tableClient
            .QueryAsync<TableEntity>(select: new List<string>() { "PartitionKey", "RowKey" }, maxPerPage: 1000, cancellationToken: cancellationToken);

        await entities.AsPages().ForEachAwaitAsync(async page =>
        // Since we don't know how many rows the table has and the results are ordered by PartitonKey+RowKey
        // we'll delete each page immediately and not cache the whole table in memory
            await BatchManipulateEntities(tableClient, page.Values, TableTransactionActionType.Delete, cancellationToken)
            .ConfigureAwait(false)
            , cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Groups entities by PartitionKey into batches of max 100 for valid transactions
    /// </summary>
    /// <returns>List of Azure Responses for Transactions</returns>
    public static async Task<List<Response<IReadOnlyList<Response>>>> BatchManipulateEntities<T>(TableClient tableClient
        , IEnumerable<T> entities
        , TableTransactionActionType tableTransactionActionType
        , CancellationToken cancellationToken) where T : class, ITableEntity, new()
    {
        var groups = entities.GroupBy(x => x.PartitionKey);
        var responses = new List<Response<IReadOnlyList<Response>>>();
        foreach (var group in groups)
        {
            List<TableTransactionAction> actions;
            var items = group.AsEnumerable();
            while (items.Any())
            {
                var batch = items.Take(100);
                items = items.Skip(100);

                actions = new List<TableTransactionAction>();
                actions.AddRange(batch.Select(e => new TableTransactionAction(tableTransactionActionType, e)));
                var response = await tableClient.SubmitTransactionAsync(actions, cancellationToken)
                    .ConfigureAwait(false);
                responses.Add(response);
            }
        }
        return responses;
    }
}
