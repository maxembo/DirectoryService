using DirectoryService.Contracts.Departments.UpdateDepartment;
using Shared.Abstractions;

namespace DirectoryService.Application.Departments.UpdateDepartments;

public record UpdateDepartmentLocationIdsCommand(Guid DepartmentId, UpdateDepartmentLocationIdsRequest Request) : ICommand;