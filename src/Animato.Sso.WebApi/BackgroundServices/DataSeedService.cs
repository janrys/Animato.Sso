namespace Animato.Sso.WebApi.BackgroundServices;

using System.Threading;
using System.Threading.Tasks;
using Animato.Sso.Application.Common.Interfaces;

public class DataSeedService : BackgroundService
{
    private readonly IDataSeeder dataSeeder;
    private readonly ILogger<DataSeedService> logger;

    public DataSeedService(IDataSeeder dataSeeder, ILogger<DataSeedService> logger)
    {
        this.dataSeeder = dataSeeder ?? throw new ArgumentNullException(nameof(dataSeeder));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation($"{nameof(DataSeedService)} starting");
        await dataSeeder.Seed();
        logger.LogInformation($"{nameof(DataSeedService)} finished");
    }
}
