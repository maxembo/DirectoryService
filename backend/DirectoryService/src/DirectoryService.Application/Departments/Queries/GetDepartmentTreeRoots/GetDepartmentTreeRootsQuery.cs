using DirectoryService.Contracts.Departments.GetDepartments.Requests;
using SharedService.Core.Abstractions;

namespace DirectoryService.Application.Departments.Queries.GetDepartmentTreeRoots;

public record GetDepartmentTreeRootsQuery(GetDepartmentTreeRootsRequest Request) : IQuery;