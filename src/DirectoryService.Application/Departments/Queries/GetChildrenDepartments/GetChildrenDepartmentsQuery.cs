using DirectoryService.Contracts.Departments.GetDepartments.Requests;
using Shared.Abstractions;

namespace DirectoryService.Application.Departments.Queries.GetChildrenDepartments;

public record GetChildrenDepartmentsQuery(Guid ParentId, GetChildrenDepartmentsRequest Request) : IQuery;