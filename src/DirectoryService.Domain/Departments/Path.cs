using CSharpFunctionalExtensions;
using Shared;

namespace DirectoryService.Domain.Departments;

public sealed record Path
{
    private const string SEPARATOR = ".";

    private Path(string value, short depth)
    {
        Value = value;
        Depth = depth;
    }

    public string Value { get; }

    public short Depth { get; }

    public static Result<Path, Error> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return GeneralErrors.Required("path");

        string[] values = value.Split(SEPARATOR);

        if (values.Any(string.IsNullOrWhiteSpace))
            return GeneralErrors.Required("path");

        short depth = (short)values.Length;

        string joinValue = string.Join(SEPARATOR, values);

        return new Path(joinValue, depth);
    }

    public static Path CreateParent(Identifier identifier)
    {
        return new Path(identifier.Value, 0);
    }

    public Path CreateChild(Identifier identifier)
    {
        string path = $"{Value}{SEPARATOR}{identifier.Value}";

        string[] depth = path.Split(SEPARATOR);

        return new Path(path, (short)(depth.Length - 1));
    }
}