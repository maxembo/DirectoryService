using CSharpFunctionalExtensions;
using DirectoryService.Domain.Locations;

namespace DirectoryService.Domain.Departments;

public class DepartmentLocation : Shared.Entity<DepartmentLocationId>
{
    // ef core
    private DepartmentLocation(DepartmentLocationId id)
        : base(id)
    { }

    private DepartmentLocation(DepartmentLocationId id, DepartmentId departmentId, LocationId locationId)
        : base(id)
    {
        DepartmentId = departmentId;
        LocationId = locationId;
    }

    public DepartmentId DepartmentId { get; private set; } = null!;

    public LocationId LocationId { get; private set; } = null!;

    public static Result<DepartmentLocation> Create(
        DepartmentLocationId id,
        DepartmentId departmentId,
        LocationId locationId)
    {
        var departmentLocation = new DepartmentLocation(id, departmentId, locationId);

        return Result.Success(departmentLocation);
    }
}