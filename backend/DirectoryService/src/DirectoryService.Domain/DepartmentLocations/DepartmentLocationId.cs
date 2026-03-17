namespace DirectoryService.Domain.DepartmentLocations;

public sealed record DepartmentLocationId
{
    private DepartmentLocationId(Guid value) => Value = value;

    public Guid Value { get; }

    public static DepartmentLocationId CreateNew() => new(Guid.NewGuid());

    public static DepartmentLocationId CreateEmpty() => new(Guid.Empty);

    public static DepartmentLocationId Create(Guid id) => new(id);
}