namespace DirectoryService.Contracts.Departments.GetDepartments.Requests;

public record GetDepartmentTreeRootsRequest(int Prefetch = 3, bool OnlyActive = false) : PaginationRequest;
