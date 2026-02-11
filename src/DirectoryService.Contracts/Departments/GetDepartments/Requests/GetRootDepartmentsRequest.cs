using DirectoryService.Contracts.Locations.GetLocations.Requests;

namespace DirectoryService.Contracts.Departments.GetDepartments.Requests;

public record GetRootDepartmentsRequest(PaginationRequest Pagination, int Prefetch = 3);