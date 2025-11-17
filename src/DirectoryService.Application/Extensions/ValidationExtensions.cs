using FluentValidation.Results;
using Shared;

namespace DirectoryService.Application.Extensions;

public static class ValidationExtensions
{
    public static Errors ToErrors(this ValidationResult validationResult) => validationResult.Errors
        .Select(e => Error.Validation(e.ErrorMessage, e.ErrorMessage, e.PropertyName))
        .ToList();
}