using DirectoryService.Application;
using DirectoryService.Infrastructure.Postgres.BackgroundServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DirectoryService.Infrastructure.Postgres.SoftDelete;

public class CleaningInactiveDepartmentsBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<CleaningInactiveDepartmentsBackgroundService> _logger;
    private readonly SoftDeleteSettings _softDeleteSettings;

    public CleaningInactiveDepartmentsBackgroundService(
        IOptions<SoftDeleteSettings> options,
        IServiceScopeFactory serviceScopeFactory,
        ILogger<CleaningInactiveDepartmentsBackgroundService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
        _softDeleteSettings = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("DeletedRecordsCleanerBackgroundService is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            int interval = BackgroundServiceHelper.GetIntervalExecute(
                _softDeleteSettings.Years, _softDeleteSettings.Months,
                _softDeleteSettings.Days,
                _softDeleteSettings.Hours,
                _softDeleteSettings.Minutes,
                _softDeleteSettings.Seconds);

            await Task.Delay(interval, stoppingToken);

            await Task.Delay(5000, stoppingToken);

            await using var scope = _serviceScopeFactory.CreateAsyncScope();

            var deletedRecordsCleanerService =
                scope.ServiceProvider.GetRequiredService<IDeleteDepartmentsService>();

            var result = await deletedRecordsCleanerService.Process(stoppingToken);

            if (result.IsSuccess)
                _logger.LogInformation("Deleted records have been deleted.");
        }

        await Task.CompletedTask;
    }
}