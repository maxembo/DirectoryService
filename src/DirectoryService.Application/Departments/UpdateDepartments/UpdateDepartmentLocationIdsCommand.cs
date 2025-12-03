using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Departments.UpdateDepartment;

namespace DirectoryService.Application.Departments.UpdateDepartments;

public record UpdateDepartmentLocationIdsCommand(Guid DepartmentId, UpdateDepartmentLocationIdsRequest Request) : ICommand;