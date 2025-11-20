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

    public Result<Path, Error> Child(string child)
    {
        if (child.Contains(SEPARATOR))
            return GeneralErrors.Invalid("path child");

        string value = $"{Value}{SEPARATOR}{child}";
        short depth = (short)(Depth + 1);

        return new Path(value, depth);
    }

    public Result<Path, Error> Parent()
    {
        string[] values = Value.Split(SEPARATOR);

        if (values.Length <= 1)
            return GeneralErrors.Invalid("path parent");

        var parent = values.Take(values.Length - 1);

        string value = string.Join(SEPARATOR, parent);
        short depth = (short)(Depth - 1);

        return new Path(value, depth);
    }
}