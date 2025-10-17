namespace DirectoryService.Domain.Departments;

public record DepartmentLocationId
{
    private DepartmentLocationId(Guid value) => Value = value;

    public Guid Value { get; }

    public static DepartmentLocationId CreateNew() => new(Guid.NewGuid());

    public static DepartmentLocationId CreateEmpty() => new(Guid.Empty);
}