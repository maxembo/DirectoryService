using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Shared;

namespace DirectoryService.Domain.Departments;

public class DepartmentPosition : Entity<DepartmentPositionId>
{
    private DepartmentPosition(
        DepartmentPositionId departmentPositionId, DepartmentId departmentId, LocationId locationId)
        : base(departmentPositionId)
    {
        DepartmentId = departmentId;
        LocationId = locationId;
    }

    public DepartmentId DepartmentId { get; private set; }

    public LocationId LocationId { get; private set; }
}