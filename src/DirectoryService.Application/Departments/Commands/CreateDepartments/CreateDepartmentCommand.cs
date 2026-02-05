using DirectoryService.Contracts.Departments.CreateDepartment;
using Shared.Abstractions;

namespace DirectoryService.Application.Departments.Commands.CreateDepartments;

public record CreateDepartmentCommand(CreateDepartmentRequest Request) : ICommand;