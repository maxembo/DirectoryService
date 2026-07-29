namespace DirectoryService.Contracts.Positions.GetPositions;

public record GetPositionsRequest(
    Guid[]? DepartmentsIds,
    string? Search,
    string? SortBy,
    string? SortDirection,
    bool? IsActive) : PaginationRequest;