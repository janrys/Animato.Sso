namespace Animato.Sso.WebApi.BackgroundServices;

using System.Threading;
using System.Threading.Tasks;
using Animato.Sso.Application.Common;
using Animato.Sso.Application.Common.Interfaces;

public class PurgeExpiredCodesService : BackgroundService
{
    private readonly OidcOptions oidcOptions;
    private readonly IAuthorizationCodeRepository authorizationCodeRepository;
    private readonly IDateTimeService dateTime;
    private readonly ILogger<PurgeExpiredCodesService> logger;
    private System.Timers.Timer timer;

    public PurgeExpiredCodesService(OidcOptions oidcOptions
        , IAuthorizationCodeRepository authorizationCodeRepository
        , IDateTimeService dateTime
        , ILogger<PurgeExpiredCodesService> logger)
    {
        this.oidcOptions = oidcOptions ?? throw new ArgumentNullException(nameof(oidcOptions));
        this.authorizationCodeRepository = authorizationCodeRepository ?? throw new ArgumentNullException(nameof(authorizationCodeRepository));
        this.dateTime = dateTime ?? throw new ArgumentNullException(nameof(dateTime));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        timer = new System.Timers.Timer(1000 * 60 * 60 * 10) // 10 hours, replace by Hangfire
        {
            AutoReset = true
        };
        timer.Elapsed += Timer_Elapsed;
        timer.Start();
        return Task.CompletedTask;
    }

    private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e) => Task.Run(() => DoWork());

    private async Task DoWork()
    {
        logger.LogInformation($"{nameof(PurgeExpiredCodesService)} starting");
        var expirationDate = dateTime.UtcNow.AddMinutes(-1 * oidcOptions.CodeExpirationMinutes);
        await authorizationCodeRepository.DeleteExpired(expirationDate, CancellationToken.None);
        logger.LogInformation($"{nameof(PurgeExpiredCodesService)} finished");
    }
}
