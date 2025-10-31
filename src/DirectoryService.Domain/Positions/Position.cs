using CSharpFunctionalExtensions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Shared;

namespace DirectoryService.Domain.Positions;

public class Position : Shared.Entity<PositionId>
{
    // ef core
    private Position(PositionId id)
        : base(id)
    { }

    private Position(PositionId id, PositionName name, string? description)
        : base(id)
    {
        Name = name;
        Description = description;
    }

    private readonly List<Department> _departments = [];

    public IReadOnlyList<Department> Departments => _departments.AsReadOnly();

    public PositionName Name { get; private set; } = null!;

    public string? Description { get; private set; }

    public static Result<Position> Create(PositionId positionId, PositionName positionName, string description)
    {
        if (string.IsNullOrWhiteSpace(description) || description.Length > Constants.MAX_POSITION_DESCRIPTION_LENGTH)
            return Result.Failure<Position>("Description cannot be empty or more than 1000 characters long.");

        var position = new Position(positionId, positionName, description);

        return Result.Success(position);
    }
}