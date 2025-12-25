using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Departments.MoveDepartments;

namespace DirectoryService.Application.Departments.MoveDepartments;

public record MoveDepartmentCommand(Guid DepartmentId, MoveDepartmentRequest Request) : ICommand;