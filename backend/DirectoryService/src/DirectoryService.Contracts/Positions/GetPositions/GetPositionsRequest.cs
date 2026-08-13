namespace DirectoryService.Contracts.Positions.GetPositions;

public record GetPositionsRequest(
    Guid[]? DepartmentIds,
    string? Search,
    string? SortBy,
    string? SortDirection,
    bool? IsActive) : PaginationRequest;