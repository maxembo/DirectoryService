using CSharpFunctionalExtensions;
using DirectoryService.Domain.Locations;
using Microsoft.Extensions.Logging;
using SharedService.Core.Abstractions;
using SharedService.Core.Database;
using SharedService.SharedKernel;

namespace DirectoryService.Application.Locations.Commands.SoftDeleteLocations;

public class SoftDeleteLocationHandler : ICommandHandler<Guid, SoftDeleteLocationCommand>
{
    private ILocationsRepository _locationRepository;
    private ITransactionManager _transactionManager;
    private ILogger<SoftDeleteLocationHandler> _logger;

    public SoftDeleteLocationHandler(
        ILocationsRepository locationRepository,
        ITransactionManager transactionManager,
        ILogger<SoftDeleteLocationHandler> logger)
    {
        _locationRepository = locationRepository;
        _transactionManager = transactionManager;
        _logger = logger;
    }

    public async Task<Result<Guid, Errors>> Handle(
        SoftDeleteLocationCommand command, CancellationToken cancellationToken = default)
    {
        var locationId = LocationId.Create(command.LocationId);

        var locationResult = await _locationRepository.GetByIdAsync(locationId, cancellationToken);
        if (locationResult.IsFailure)
        {
            return locationResult.Error.ToErrors();
        }

        var location = locationResult.Value;

        location.MarkAsDelete();

        var transactionResult = await _transactionManager.SaveChangeAsync(cancellationToken);
        if (transactionResult.IsFailure)
        {
            return transactionResult.Error.ToErrors();
        }

        _logger.LogInformation("Location {Location.id} soft deleted successfully.", location.Id.Value);

        return location.Id.Value;
    }
}