namespace DirectoryService.Contracts.Locations.GetLocations.Requests;

public record GetLocationsRequest(
    Guid[]? DepartmentIds,
    string? Search,
    bool? IsActive,
    string? SortBy,
    string? SortDirection) : PaginationRequest;