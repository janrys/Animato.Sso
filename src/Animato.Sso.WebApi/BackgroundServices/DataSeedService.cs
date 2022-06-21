namespace Animato.Sso.WebApi.BackgroundServices;

using System.Threading;
using System.Threading.Tasks;
using Animato.Sso.Application.Common.Interfaces;
using Animato.Sso.Application.Common.Logging;

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
        logger.ServiceStartingInformation(nameof(DataSeedService));
        await dataSeeder.Seed();
        logger.ServiceFinishedInformation(nameof(DataSeedService));
    }
}
