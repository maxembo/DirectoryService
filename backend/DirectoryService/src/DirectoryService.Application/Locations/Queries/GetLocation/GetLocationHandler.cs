using CSharpFunctionalExtensions;
using DirectoryService.Contracts.Locations.GetLocations.Dtos;
using DirectoryService.Domain.Locations;
using SharedService.Core.Abstractions;
using SharedService.SharedKernel;

namespace DirectoryService.Application.Locations.Queries.GetLocation;

public class GetLocationHandler : IQueryHandler<GetLocationsDto, GetLocationQuery>
{
    private readonly ILocationsRepository _locationsRepository;

    public GetLocationHandler(ILocationsRepository locationsRepository)
    {
        _locationsRepository = locationsRepository;
    }

    public async Task<Result<GetLocationsDto, Errors>> Handle(
        GetLocationQuery query, CancellationToken cancellationToken)
    {
        var locationId = LocationId.Create(query.LocationId);

        var locationResult = await _locationsRepository.GetByIdIncludingInactiveAsync(locationId, cancellationToken);
        if (locationResult.IsFailure)
        {
            return locationResult.Error.ToErrors();
        }

        var location = locationResult.Value;

        var result = new GetLocationsDto
        {
            Id = location.Id.Value,
            Name = location.Name.Value,
            Address = new AddressDto(
                location.Address.City, location.Address.Country, location.Address.Street, location.Address.House),
            CreatedAt = location.CreatedAt,
            UpdatedAt = location.UpdatedAt,
            IsActive = location.IsActive,
            Timezone = location.Timezone.Value,
            DeletedAt = location.DeletedAt,
        };

        return result;
    }
}