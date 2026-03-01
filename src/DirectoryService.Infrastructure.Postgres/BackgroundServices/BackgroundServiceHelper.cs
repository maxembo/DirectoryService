namespace DirectoryService.Infrastructure.Postgres.BackgroundServices;

public static class BackgroundServiceHelper
{
    public static int GetIntervalExecute(
        int years,
        int months,
        int days,
        int hours,
        int minutes,
        int seconds)
    {
        var now = DateTime.Now;

        var targetTime = new DateTime(now.Year, now.Month, now.Day, hours, minutes, seconds);

        if (now > targetTime)
        {
            targetTime = targetTime
                .AddYears(years)
                .AddMonths(months)
                .AddDays(days);
        }

        var interval = targetTime - now;

        if (interval.TotalMilliseconds > int.MaxValue)
            throw new ArgumentException("Интервал слишком большой");

        return (int)interval.TotalMilliseconds;
    }
}