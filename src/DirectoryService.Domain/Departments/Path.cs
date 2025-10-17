using CSharpFunctionalExtensions;

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

    public static Result<Path> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            Result.Failure<Path>("Path should not be empty.");

        string[] values = value.Split(SEPARATOR);

        if (values.Any(string.IsNullOrWhiteSpace))
            return Result.Failure<Path>("Path value should not be empty.");

        short depth = (short)values.Length;

        string joinValue = string.Join(SEPARATOR, values);

        var path = new Path(joinValue, depth);

        return Result.Success(path);
    }

    public Result<Path> Child(string child)
    {
        if (child.Contains(SEPARATOR))
            return Result.Failure<Path>("Path child cannot contain the separator");

        string value = $"{Value}{SEPARATOR}{child}";
        short depth = (short)(Depth + 1);

        var path = new Path(value, depth);
        return Result.Success(path);
    }

    public Result<Path> Parent()
    {
        var values = Value.Split(SEPARATOR);

        var parent = values.Take(values.Length - 1);

        string value = string.Join(SEPARATOR, parent);
        short depth = (short)(Depth - 1);

        var path = new Path(value, depth);
        return Result.Success(path);
    }
}