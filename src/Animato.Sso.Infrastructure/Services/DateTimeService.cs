namespace Animato.Sso.Infrastructure.Services;
using Animato.Sso.Application.Common.Interfaces;

public class DateTimeService : IDateTimeService
{
    public DateTime Now => DateTime.Now;

    public DateTime UtcNow => DateTime.UtcNow;

    public DateTimeOffset UtcNowOffset => DateTimeOffset.UtcNow;
}
