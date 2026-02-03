using DirectoryService.Contracts.Locations.GetLocations.Dtos;

namespace DirectoryService.Contracts.Locations.GetLocations;

public record PaginationLocationResponse(IReadOnlyList<GetLocationsDto> Locations, long TotalCount);