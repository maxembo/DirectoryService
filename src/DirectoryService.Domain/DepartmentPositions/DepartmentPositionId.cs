namespace DirectoryService.Domain.DepartmentPositions;

public sealed record DepartmentPositionId
{
    private DepartmentPositionId(Guid value) => Value = value;

    public Guid Value { get; }

    public static DepartmentPositionId CreateNew() => new(Guid.NewGuid());

    public static DepartmentPositionId CreateEmpty() => new(Guid.Empty);

    public static DepartmentPositionId Create(Guid id) => new(id);
}