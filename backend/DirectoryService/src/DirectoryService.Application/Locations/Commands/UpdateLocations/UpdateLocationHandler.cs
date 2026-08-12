using CSharpFunctionalExtensions;
using DirectoryService.Contracts.Locations.UpdateLocations;
using DirectoryService.Domain.Locations;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using SharedService.Core.Abstractions;
using SharedService.Core.Database;
using SharedService.Core.Validation;
using SharedService.SharedKernel;

namespace DirectoryService.Application.Locations.Commands.UpdateLocations;

public class UpdateLocationHandler : ICommandHandler<Guid, UpdateLocationCommand>
{
    private readonly ILocationsRepository _locationsRepository;
    private readonly ILogger<UpdateLocationHandler> _logger;
    private readonly ITransactionManager _transactionManager;
    private readonly IValidator<UpdateLocationRequest> _validator;

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
        ValidationResult? validationResult = await _validator.ValidateAsync(command.Request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToErrors();
        }

        var locationId = LocationId.Create(command.LocationId);

        Result<Location, Error> locationResult = await _locationsRepository.GetByIdAsync(locationId, cancellationToken);
        if (locationResult.IsFailure)
        {
            return locationResult.Error.ToErrors();
        }

        Location? location = locationResult.Value;

        LocationName? name = LocationName.Create(command.Request.Name).Value;
        Timezone? timezone = Timezone.Create(command.Request.Timezone).Value;
        Address? address = Address.Create(
            command.Request.Address.City,
            command.Request.Address.Country,
            command.Request.Address.Street,
            command.Request.Address.House).Value;

        location.Update(name, timezone, address);

        UnitResult<Error> transactionResult = await _transactionManager.SaveChangeAsync(cancellationToken);
        if (transactionResult.IsFailure)
        {
            return transactionResult.Error.ToErrors();
        }

        _logger.LogInformation("Location with id: {Location.Id} has been updated.", location.Id.Value);

        return location.Id.Value;
    }
}