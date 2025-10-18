namespace DirectoryService.Domain.Departments;

public record DepartmentPositionId
{
    private DepartmentPositionId(Guid value) => Value = value;

    public Guid Value { get; }

    public static DepartmentPositionId CreateNew() => new(Guid.NewGuid());

    public static DepartmentPositionId CreateEmpty() => new(Guid.Empty);
}