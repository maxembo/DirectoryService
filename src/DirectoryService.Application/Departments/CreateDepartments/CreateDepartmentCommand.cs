using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Departments.CreateDepartment;

namespace DirectoryService.Application.Departments.CreateDepartments;

public record CreateDepartmentCommand(CreateDepartmentRequest Request) : ICommand;