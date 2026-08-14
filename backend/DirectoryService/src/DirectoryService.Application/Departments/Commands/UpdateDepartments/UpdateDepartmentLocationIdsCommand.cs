using DirectoryService.Contracts.Departments.UpdateDepartments;
using SharedService.Core.Abstractions;

namespace DirectoryService.Application.Departments.Commands.UpdateDepartments;

public record UpdateDepartmentLocationIdsCommand(Guid DepartmentId, UpdateDepartmentLocationIdsRequest Request)
    : ICommand;