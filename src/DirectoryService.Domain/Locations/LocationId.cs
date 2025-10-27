namespace DirectoryService.Domain.Locations;

public record LocationId
{
    private LocationId(Guid value) => Value = value;

    public Guid Value { get; }

    public static LocationId CreateNew() => new(Guid.NewGuid());

    public static LocationId CreateEmpty() => new(Guid.Empty);

    public static LocationId Create(Guid id) => new(id);
}