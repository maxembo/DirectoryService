using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Locations.CreateLocations;
using DirectoryService.Domain.Locations;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Shared;

namespace DirectoryService.Application.Locations.CreateLocation;

public class CreateLocationHandler : ICommandHandler<Guid, CreateLocationRequest>
{
    private readonly ILocationsRepository _locationsRepository;
    private readonly IValidator<CreateLocationDto> _validator;
    private readonly ILogger<CreateLocationHandler> _logger;

    public CreateLocationHandler(
        ILocationsRepository locationsRepository,
        IValidator<CreateLocationDto> validator,
        ILogger<CreateLocationHandler> logger)
    {
        _locationsRepository = locationsRepository;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<Guid, Errors>> Handle(
        CreateLocationRequest request, CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(request.LocationDto, cancellationToken);
        if (!validationResult.IsValid)
        {
            return GeneralErrors
                .Invalid("location")
                .ToErrors();
        }

        var location = new Location(
            LocationId.CreateNew(),
            LocationName.Create(request.LocationDto.Name).Value,
            Timezone.Create(request.LocationDto.Timezone).Value,
            Address.Create(
                request.LocationDto.Address.City,
                request.LocationDto.Address.Country,
                request.LocationDto.Address.Street,
                request.LocationDto.Address.House).Value);

        await _locationsRepository.AddAsync(location, cancellationToken);

        _logger.LogInformation("Location {Location.Id} has been created.", location.Id);

        return location.Id.Value;
    }
}