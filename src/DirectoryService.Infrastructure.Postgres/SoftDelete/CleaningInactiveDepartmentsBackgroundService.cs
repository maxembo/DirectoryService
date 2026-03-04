using CSharpFunctionalExtensions;
using DirectoryService.Application;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared;

namespace DirectoryService.Infrastructure.Postgres.SoftDelete;

public class CleaningInactiveDepartmentsBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<CleaningInactiveDepartmentsBackgroundService> _logger;
    private readonly InactiveDepartmentsCleanupOptions _options;

    public CleaningInactiveDepartmentsBackgroundService(
        IOptions<InactiveDepartmentsCleanupOptions> options,
        IServiceScopeFactory serviceScopeFactory,
        ILogger<CleaningInactiveDepartmentsBackgroundService> logger)
    {
        _options = options.Value;
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("CleaningInactiveDepartmentsBackgroundService is starting.");

        using var timer = new PeriodicTimer(_options.Interval);

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                var result = await DeleteInactiveDepartmentsAsync(stoppingToken);

                if (result.IsSuccess)
                    _logger.LogInformation("Deleted records have been deleted.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CleaningInactiveDepartmentsBackgroundService failed.");
        }

        await Task.CompletedTask;
    }

    private async Task<UnitResult<Error>> DeleteInactiveDepartmentsAsync(CancellationToken stoppingToken)
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();

        var deletedRecordsCleanerService =
            scope.ServiceProvider.GetRequiredService<IDeleteDepartmentsService>();

        return await deletedRecordsCleanerService.Process(stoppingToken);
    }
}