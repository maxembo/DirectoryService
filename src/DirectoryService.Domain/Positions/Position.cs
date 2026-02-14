using DirectoryService.Domain.DepartmentPositions;
using Shared;

namespace DirectoryService.Domain.Positions;

public sealed class Position : Shared.Entity<PositionId>, ISoftDeletable
{
    // ef core
    private Position(PositionId id)
        : base(id)
    { }

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

    private readonly List<DepartmentPosition> _departments = [];

    public IReadOnlyList<DepartmentPosition> Departments => _departments.AsReadOnly();

    public PositionName Name { get; private set; } = null!;

    public Description? Description { get; private set; }

    public bool IsActive { get; private set; } = true;

    public DateTime? DeletedAt { get; private set; }

    public void MarkAsDelete()
    {
        IsActive = false;
        DeletedAt = DateTime.UtcNow;
    }
}