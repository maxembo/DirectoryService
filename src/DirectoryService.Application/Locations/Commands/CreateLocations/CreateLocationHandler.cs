using CSharpFunctionalExtensions;
using DirectoryService.Contracts.Locations.CreateLocations;
using DirectoryService.Domain.Locations;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Shared;
using Shared.Abstractions;
using Shared.Validation;

namespace DirectoryService.Application.Locations.Commands.CreateLocations;

public class CreateLocationHandler : ICommandHandler<Guid, CreateLocationCommand>
{
    private readonly ILocationsRepository _locationsRepository;
    private readonly IValidator<CreateLocationRequest> _validator;
    private readonly ILogger<CreateLocationHandler> _logger;

    public CreateLocationHandler(
        ILocationsRepository locationsRepository,
        IValidator<CreateLocationRequest> validator,
        ILogger<CreateLocationHandler> logger)
    {
        _locationsRepository = locationsRepository;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<Guid, Errors>> Handle(
        CreateLocationCommand command, CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(command.LocationRequest, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.ToErrors();
            _logger.LogWarning("Validation failed for LocationRequest: {Errors}", errors);
            return errors;
        }

        var location = new Location(
            LocationId.CreateNew(),
            LocationName.Create(command.LocationRequest.Name).Value,
            Timezone.Create(command.LocationRequest.Timezone).Value,
            Address.Create(
                command.LocationRequest.Address.City,
                command.LocationRequest.Address.Country,
                command.LocationRequest.Address.Street,
                command.LocationRequest.Address.House).Value);

        var repositoryResult = await _locationsRepository.AddAsync(location, cancellationToken);
        if (repositoryResult.IsFailure)
        {
            return Error.Failure(null, repositoryResult.Error.Message)
                .ToErrors();
        }

        _logger.LogInformation("Location {Location.Id} has been created.", location.Id);

        return location.Id.Value;
    }
}