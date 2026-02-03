namespace DirectoryService.Contracts.Locations.GetLocations.Requests;

public record GetLocationsRequest(
    Guid[]? DepartmentIds,
    string? Search,
    bool? IsActive,
    PaginationRequest Pagination,
    string? SortBy,
    string? SortDirection);