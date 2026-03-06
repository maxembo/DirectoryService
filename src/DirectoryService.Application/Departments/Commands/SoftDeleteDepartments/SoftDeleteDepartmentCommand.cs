using Shared.Core.Abstractions;

namespace DirectoryService.Application.Departments.Commands.SoftDeleteDepartments;

public record SoftDeleteDepartmentCommand(Guid DepartmentId) : ICommand;