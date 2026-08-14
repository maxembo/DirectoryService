namespace DirectoryService.Contracts.Departments.CreateDepartments;

public record CreateDepartmentRequest(
    string Name,
    string Identifier,
    Guid? ParentId,
    Guid[] LocationIds);