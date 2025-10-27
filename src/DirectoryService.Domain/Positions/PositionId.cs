namespace DirectoryService.Domain.Positions;

public record PositionId
{
    private PositionId(Guid value) => Value = value;

    public Guid Value { get; }

    public static PositionId CreateNew() => new(Guid.NewGuid());

    public static PositionId CreateEmpty() => new(Guid.Empty);

    public static PositionId Create(Guid id) => new(id);
}