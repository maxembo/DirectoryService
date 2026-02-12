using DirectoryService.Contracts.Departments.GetDepartments;
using DirectoryService.Contracts.Departments.GetDepartments.Requests;
using Shared.Abstractions;

namespace DirectoryService.Application.Departments.Queries.GetRootDepartments;

public record GetRootDepartmentsQuery(GetRootDepartmentsRequest Request) : IQuery;