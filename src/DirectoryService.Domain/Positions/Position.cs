using CSharpFunctionalExtensions;
using DirectoryService.Domain.DepartmentPositions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Shared;
using Shared;

namespace DirectoryService.Domain.Positions;

public sealed class Position : Shared.Entity<PositionId>
{
    // ef core
    private Position(PositionId id)
        : base(id)
    { }

    public Position(PositionId id, PositionName name, Description? description)
        : base(id)
    {
        Name = name;
        Description = description;
    }

    private readonly List<DepartmentPosition> _departmentPositions = [];

    public IReadOnlyList<DepartmentPosition> DepartmentPositions => _departmentPositions.AsReadOnly();

    public PositionName Name { get; private set; } = null!;

    public Description? Description { get; private set; }
}