using DirectoryService.Contracts.Locations.GetLocations.Requests;

namespace DirectoryService.Contracts.Departments.GetDepartments.Requests;

public record GetChildrenDepartmentsRequest(PaginationRequest Pagination);