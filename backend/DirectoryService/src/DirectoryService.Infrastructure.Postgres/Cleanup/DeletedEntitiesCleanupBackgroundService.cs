using CSharpFunctionalExtensions;
using DirectoryService.Application.Cleanup;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedService.SharedKernel;

namespace DirectoryService.Infrastructure.Postgres.Cleanup;

public class DeletedEntitiesCleanupBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<DeletedEntitiesCleanupBackgroundService> _logger;
    private readonly DeletedEntitiesCleanupOptions _options;

    public DeletedEntitiesCleanupBackgroundService(
        IOptions<DeletedEntitiesCleanupOptions> options,
        IServiceScopeFactory serviceScopeFactory,
        ILogger<DeletedEntitiesCleanupBackgroundService> logger)
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
        catch (OperationCanceledException)
            when (stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("CleaningInactiveDepartmentsBackgroundService is canceled.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CleaningInactiveDepartmentsBackgroundService failed.");
        }
    }

    private async Task<UnitResult<Error>> DeleteInactiveDepartmentsAsync(CancellationToken stoppingToken)
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();

        var deletedRecordsCleanerService =
            scope.ServiceProvider.GetRequiredService<IDeletedEntitiesCleanupService>();

        return await deletedRecordsCleanerService.Process(stoppingToken);
    }
}