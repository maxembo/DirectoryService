using DirectoryService.Contracts.Departments.GetDepartments.Requests;
using SharedService.Core.Abstractions;

namespace DirectoryService.Application.Departments.Queries.GetDepartmentChildren;

public record GetDepartmentChildrenQuery(Guid ParentId, GetDepartmentChildrenRequest Request) : IQuery;