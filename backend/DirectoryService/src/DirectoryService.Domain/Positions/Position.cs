using DirectoryService.Domain.DepartmentPositions;
using SharedService.SharedKernel;

namespace DirectoryService.Domain.Positions;

public sealed class Position : BaseEntity<PositionId>, ISoftDeletable
{
    private readonly List<DepartmentPosition> _departments = [];

    public Position(
        PositionId id,
        PositionName name,
        Description? description,
        IEnumerable<DepartmentPosition> departments)
        : base(id)
    {
        Name = name;
        Description = description;
        _departments = departments.ToList();
    }

    // ef core
    private Position(PositionId id)
        : base(id)
    { }

    public IReadOnlyList<DepartmentPosition> Departments => _departments.AsReadOnly();

    public PositionName Name { get; private set; } = null!;

    public Description? Description { get; private set; }

    public DeletionReason? DeletionReason { get; private set; }

    public bool IsActive { get; private set; } = true;

    public DateTime? DeletedAt { get; private set; }

    public void MarkAsDelete() => ApplySoftDelete(Domain.DeletionReason.MANUAL);

    public void Update(PositionName name, Description? description)
    {
        Name = name;
        Description = description;

        UpdatedAt = DateTime.UtcNow;
    }

    private void ApplySoftDelete(DeletionReason reason)
    {
        DateTime now = DateTime.UtcNow;
        IsActive = false;
        DeletedAt = now;
        DeletionReason = reason;

        UpdatedAt = now;
    }
}