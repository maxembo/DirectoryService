using DirectoryService.Contracts.Departments.UpdateDepartment;
using SharedService.Core.Abstractions;

namespace DirectoryService.Application.Departments.Commands.UpdateDepartments;

public record UpdateDepartmentLocationIdsCommand(Guid DepartmentId, UpdateDepartmentLocationIdsRequest Request) : ICommand;