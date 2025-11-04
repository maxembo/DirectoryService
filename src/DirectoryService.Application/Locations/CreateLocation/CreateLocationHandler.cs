using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Locations;
using DirectoryService.Domain.Locations;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Locations.CreateLocation;

public class CreateLocationHandler : ICommandHandler<Guid, CreateLocationRequest>
{
    private readonly ILocationsRepository _locationsRepository;
    private readonly IValidator<CreateLocationDto> _validator;
    private readonly ILogger<CreateLocationHandler> _logger;

    public CreateLocationHandler(
        ILocationsRepository locationsRepository, IValidator<CreateLocationDto> validator,
        ILogger<CreateLocationHandler> logger)
    {
        _locationsRepository = locationsRepository;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(CreateLocationRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var validationResult = await _validator.ValidateAsync(request.LocationDto, cancellationToken);
            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            var locationResult = Location.Create(
                LocationName.Create(request.LocationDto.Name).Value,
                Timezone.Create(request.LocationDto.Timezone).Value,
                Address.Create(
                    request.LocationDto.Address.City,
                    request.LocationDto.Address.Country,
                    request.LocationDto.Address.Street,
                    request.LocationDto.Address.House).Value);

            if (locationResult.IsFailure)
                return Result.Failure<Guid>(locationResult.Error);

            await _locationsRepository.AddAsync(locationResult.Value, cancellationToken);

            _logger.LogInformation($"Location {locationResult.Value.Id} has been created.");

            return Result.Success(locationResult.Value.Id.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create location!");
            return Result.Failure<Guid>(ex.Message);
        }
    }
}