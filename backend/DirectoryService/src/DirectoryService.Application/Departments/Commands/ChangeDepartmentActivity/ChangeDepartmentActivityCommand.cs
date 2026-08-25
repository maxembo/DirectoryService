using SharedService.Core.Abstractions;

namespace DirectoryService.Application.Departments.Commands.ChangeDepartmentActivity;

public record ChangeDepartmentActivityCommand(Guid DepartmentId, bool IsActive) : ICommand;
