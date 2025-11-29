using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Departments.CreateDepartment;

namespace DirectoryService.Application.Departments.CreateDepartment;

public record CreateDepartmentCommand(CreateDepartmentRequest Request) : ICommand;
