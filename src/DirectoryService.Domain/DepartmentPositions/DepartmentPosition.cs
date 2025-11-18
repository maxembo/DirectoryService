using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Positions;

namespace DirectoryService.Domain.DepartmentPositions;

public sealed class DepartmentPosition : Shared.Entity<DepartmentPositionId>
{
    // ef core
    private DepartmentPosition(DepartmentPositionId id)
        : base(id)
    { }

    public DepartmentPosition(DepartmentPositionId id, DepartmentId departmentId, PositionId positionId)
        : base(id)
    {
        DepartmentId = departmentId;
        PositionId = positionId;
    }

    public DepartmentId DepartmentId { get; private set; } = null!;

    public PositionId PositionId { get; private set; } = null!;
}