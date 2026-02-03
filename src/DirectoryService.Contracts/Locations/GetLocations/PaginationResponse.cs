namespace DirectoryService.Contracts.Locations.GetLocations;

public record PaginationResponse(IReadOnlyList<GetLocationsDto> Locations, long TotalCount);