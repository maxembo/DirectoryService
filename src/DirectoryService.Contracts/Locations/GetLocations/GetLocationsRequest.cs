using DirectoryService.Domain.Departments;

namespace DirectoryService.Contracts.Locations.GetLocations;

public record GetLocationsRequest(
    Guid[]? DepartmentIds,
    string? Search,
    bool? IsActive,
    PaginationRequest Pagination,
    string? SortBy,
    string? SortDirection);