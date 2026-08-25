using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using DirectoryService.Domain.Shared;
using SharedService.SharedKernel;

namespace DirectoryService.Domain.Departments;

public sealed record Identifier
{
    private const string ARCHIVED_PATH_PREFIX = "delete-";

    private static readonly Regex _identifierRegex =
        new("^[A-Za-z-]+$", RegexOptions.Compiled);

    private Identifier(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<Identifier, Error> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return GeneralErrors.Required("department.identifier");
        }

        if (!_identifierRegex.IsMatch(value))
        {
            return GeneralErrors.MismatchRegex("department.identifier");
        }

        if (value.StartsWith(ARCHIVED_PATH_PREFIX, StringComparison.Ordinal))
        {
            return GeneralErrors.MismatchRegex("department.identifier");
        }

        if (value.Length is < Constants.MIN_TEXT_LENGTH or > Constants.MAX_DEPARTMENT_IDENTIFIER_LENGTH)
        {
            return GeneralErrors.LengthOutOfRange(
                "department.identifier", Constants.MIN_TEXT_LENGTH, Constants.MAX_DEPARTMENT_IDENTIFIER_LENGTH);
        }

        return new Identifier(value);
    }
}
