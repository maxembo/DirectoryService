using CSharpFunctionalExtensions;
using Shared;

namespace DirectoryService.Domain.Departments;

public record Path
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
            return GeneralErrors.ValueIsInvalid("path");

        string[] values = value.Split(SEPARATOR);

        if (values.Any(string.IsNullOrWhiteSpace))
            return GeneralErrors.ValueIsInvalid("path");

        short depth = (short)values.Length;

        string joinValue = string.Join(SEPARATOR, values);

        var path = new Path(joinValue, depth);

        return path;
    }

    public Result<Path, Error> Child(string child)
    {
        if (child.Contains(SEPARATOR))
            return GeneralErrors.ValueIsInvalid("path");

        string value = $"{Value}{SEPARATOR}{child}";
        short depth = (short)(Depth + 1);

        var path = new Path(value, depth);
        return path;
    }

    public Result<Path, Error> Parent()
    {
        var values = Value.Split(SEPARATOR);

        if (values.Count() <= 1)
            return GeneralErrors.ValueIsInvalid("path");

        var parent = values.Take(values.Length - 1);

        string value = string.Join(SEPARATOR, parent);
        short depth = (short)(Depth - 1);

        var path = new Path(value, depth);
        return path;
    }
}