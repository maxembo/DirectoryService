using DirectoryService.Contracts.Departments.MoveDepartments;
using Shared.Abstractions;

namespace DirectoryService.Application.Departments.MoveDepartments;

public record MoveDepartmentCommand(Guid DepartmentId, MoveDepartmentRequest Request) : ICommand;