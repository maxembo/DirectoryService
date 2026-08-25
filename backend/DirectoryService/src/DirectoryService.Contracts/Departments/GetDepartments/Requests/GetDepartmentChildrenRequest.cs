namespace DirectoryService.Contracts.Departments.GetDepartments.Requests;

public record GetDepartmentChildrenRequest(bool OnlyActive = false) : PaginationRequest;
