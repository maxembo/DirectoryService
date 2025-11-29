using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using DirectoryService.Domain.Shared;
using Shared;

namespace DirectoryService.Domain.Departments;

public sealed record Identifier
{
    private static readonly Regex _identifierRegex =
        new("^[A-Za-z]+$", RegexOptions.Compiled);

    private Identifier(string value) => Value = value;

    public string Value { get; }

    public static Result<Identifier, Error> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return GeneralErrors.Required("identifier");
        }

        if (!_identifierRegex.IsMatch(value))
        {
            return GeneralErrors.MismatchRegex("identifier");
        }

        if (value.Length is < Constants.MIN_TEXT_LENGTH or > Constants.MAX_DEPARTMENT_IDENTIFIER_LENGTH)
        {
            return GeneralErrors.LengthOutOfRange(
                "identifier", Constants.MAX_DEPARTMENT_IDENTIFIER_LENGTH, Constants.MIN_TEXT_LENGTH);
        }

        return new Identifier(value);
    }
}