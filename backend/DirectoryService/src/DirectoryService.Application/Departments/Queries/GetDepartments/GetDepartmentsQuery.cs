using DirectoryService.Contracts.Departments.GetDepartments.Requests;
using SharedService.Core.Abstractions;

namespace DirectoryService.Application.Departments.Queries.GetDepartments;

public record GetDepartmentsQuery(GetDepartmentsRequest Request) : IQuery;