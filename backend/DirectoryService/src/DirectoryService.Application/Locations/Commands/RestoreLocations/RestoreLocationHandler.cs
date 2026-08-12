using CSharpFunctionalExtensions;
using DirectoryService.Domain.Locations;
using Microsoft.Extensions.Logging;
using SharedService.Core.Abstractions;
using SharedService.Core.Database;
using SharedService.SharedKernel;

namespace DirectoryService.Application.Locations.Commands.RestoreLocations;

public class RestoreLocationHandler : ICommandHandler<Guid, RestoreLocationCommand>
{
    private readonly ILocationsRepository _locationsRepository;
    private readonly ILogger<RestoreLocationHandler> _logger;
    private readonly ITransactionManager _transactionManager;

    public RestoreLocationHandler(
        ILocationsRepository locationsRepository,
        ITransactionManager transactionManager,
        ILogger<RestoreLocationHandler> logger)
    {
        _locationsRepository = locationsRepository;
        _transactionManager = transactionManager;
        _logger = logger;
    }

    public async Task<Result<Guid, Errors>> Handle(RestoreLocationCommand command, CancellationToken cancellationToken)
    {
        var locationId = LocationId.Create(command.LocationId);

        Result<Location, Error> locationResult =
            await _locationsRepository.GetByIdIncludingInactiveAsync(locationId, cancellationToken);
        if (locationResult.IsFailure)
        {
            return locationResult.Error.ToErrors();
        }

        Location? location = locationResult.Value;

        if (location.IsActive)
        {
            _logger.LogInformation(
                "Location {LocationId} is already active.",
                location.Id.Value);

            return location.Id.Value;
        }

        location.Restore();

        UnitResult<Error> saveResult = await _transactionManager.SaveChangeAsync(cancellationToken);
        if (saveResult.IsFailure)
        {
            return saveResult.Error.ToErrors();
        }

        _logger.LogInformation("Location {LocationId} restored successfully.", location.Id.Value);

        return location.Id.Value;
    }
}