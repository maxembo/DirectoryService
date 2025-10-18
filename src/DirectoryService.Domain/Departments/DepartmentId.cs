namespace DirectoryService.Domain.Departments;

public record DepartmentId
{
    private DepartmentId(Guid value) => Value = value;

    public Guid Value { get; }

    public static DepartmentId CreateNew() => new(Guid.NewGuid());

    public static DepartmentId CreateEmpty() => new(Guid.Empty);
}