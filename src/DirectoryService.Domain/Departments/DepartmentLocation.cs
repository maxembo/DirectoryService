using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Shared;

namespace DirectoryService.Domain.Departments;

public class DepartmentLocation : Entity<DepartmentLocationId>
{
    private DepartmentLocation(
        DepartmentLocationId departmentLocationId, DepartmentId departmentId, LocationId locationId)
        : base(departmentLocationId)
    {
        Department = departmentId;
        LocationId = locationId;
    }

    public DepartmentId Department { get; private set; }

    public LocationId LocationId { get; private set; }
}