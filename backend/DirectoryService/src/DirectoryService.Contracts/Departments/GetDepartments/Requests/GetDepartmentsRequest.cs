namespace DirectoryService.Contracts.Departments.GetDepartments.Requests;

public record GetDepartmentsRequest(
    Guid[]? LocationsIds,
    string? Search,
    bool? IsActive,
    Guid? ParentId,
    bool? IsParent,
    string? SortBy,
    string? SortDirection) : PaginationRequest;