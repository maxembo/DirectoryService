using SharedService.Core.Abstractions;

namespace DirectoryService.Application.Departments.Commands.RestoreDepartments;

public record RestoreDepartmentCommand(Guid DepartmentId) : ICommand;