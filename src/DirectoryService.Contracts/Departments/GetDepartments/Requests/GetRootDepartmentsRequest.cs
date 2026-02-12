namespace DirectoryService.Contracts.Departments.GetDepartments.Requests;

public record GetRootDepartmentsRequest(int Prefetch = 3) : PaginationRequest;