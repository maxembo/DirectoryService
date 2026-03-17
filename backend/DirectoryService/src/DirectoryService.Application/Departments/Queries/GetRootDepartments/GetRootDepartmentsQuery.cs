using DirectoryService.Contracts.Departments.GetDepartments.Requests;
using SharedService.Core.Abstractions;

namespace DirectoryService.Application.Departments.Queries.GetRootDepartments;

public record GetRootDepartmentsQuery(GetRootDepartmentsRequest Request) : IQuery;