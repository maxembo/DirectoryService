using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using DirectoryService.Domain.Shared;

namespace DirectoryService.Domain.Departments;

public record Identifier
{
    private static readonly Regex _identifierRegex =
        new Regex("^[A-Za-z]+$", RegexOptions.Compiled);

    private Identifier(string value) => Value = value;

    public string Value { get; }

    public static Result<Identifier> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value) ||
            value.Length < Constants.MIN_TEXT_LENGTH ||
            value.Length > Constants.MAX_DEPARTMENT_IDENTIFIER_LENGTH ||
            !_identifierRegex.IsMatch(value))
        {
            return Result.Failure<Identifier>(
                "Identifier must be at least 3 characters long and no more than 150 characters long.");
        }

        var identifier = new Identifier(value);

        return Result.Success(identifier);
    }
}