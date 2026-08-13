using DirectoryService.Contracts.Departments.CreateDepartments;
using SharedService.Core.Abstractions;

namespace DirectoryService.Application.Departments.Commands.CreateDepartments;

public record CreateDepartmentCommand(CreateDepartmentRequest Request) : ICommand;