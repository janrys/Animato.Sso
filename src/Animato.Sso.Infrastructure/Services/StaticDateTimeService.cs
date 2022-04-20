namespace Animato.Sso.Infrastructure.Services;
using Animato.Sso.Application.Common.Interfaces;

public class StaticDateTimeService : IDateTimeService
{
    private readonly DateTime value;

    public StaticDateTimeService(DateTime value) => this.value = value;

    public DateTime Now => value.ToLocalTime();

    public DateTime UtcNow => value.ToUniversalTime();
}
