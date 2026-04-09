using CSharpFunctionalExtensions;
using DirectoryService.Contracts.Locations.UpdateLocations;
using DirectoryService.Domain.Locations;
using FluentValidation;
using Microsoft.Extensions.Logging;
using SharedService.Core.Abstractions;
using SharedService.Core.Database;
using SharedService.Core.Validation;
using SharedService.SharedKernel;

namespace DirectoryService.Application.Locations.Commands.UpdateLocations;

public class UpdateLocationHandler : ICommandHandler<Guid, UpdateLocationCommand>
{
    private readonly ILocationsRepository _locationsRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly IValidator<UpdateLocationRequest> _validator;
    private readonly ILogger<UpdateLocationHandler> _logger;

    public UpdateLocationHandler(
        ILocationsRepository locationsRepository,
        ITransactionManager transactionManager,
        IValidator<UpdateLocationRequest> validator,
        ILogger<UpdateLocationHandler> logger)
    {
        _locationsRepository = locationsRepository;
        _transactionManager = transactionManager;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<Guid, Errors>> Handle(
        UpdateLocationCommand command, CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(command.Request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToErrors();
        }

        var locationId = LocationId.Create(command.LocationId);

        var locationResult = await _locationsRepository.GetByIdAsync(locationId, cancellationToken);
        if (locationResult.IsFailure)
        {
            return locationResult.Error.ToErrors();
        }

        var location = locationResult.Value;

        var name = LocationName.Create(command.Request.Name).Value;
        var timezone = Timezone.Create(command.Request.Timezone).Value;
        var address = Address.Create(
            command.Request.Address.City,
            command.Request.Address.Country,
            command.Request.Address.Street,
            command.Request.Address.House).Value;

        location.Update(name, timezone, address);

        var transactionResult = await _transactionManager.SaveChangeAsync(cancellationToken);
        if (transactionResult.IsFailure)
        {
            return transactionResult.Error.ToErrors();
        }

        _logger.LogInformation("Location with id: {Location.Id} has been updated.", location.Id.Value);

        return location.Id.Value;
    }
}