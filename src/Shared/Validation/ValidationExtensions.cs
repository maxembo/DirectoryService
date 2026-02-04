using System.Text.Json;
using FluentValidation.Results;

namespace Shared.Validation;

public static class ValidationExtensions
{
    // public static Errors ToErrors(this ValidationResult validationResult) => validationResult.Errors
    //     .Select(e => Error.Validation(e.ErrorMessage, e.ErrorMessage, e.PropertyName))
    //     .ToList();

    public static Errors ToErrors(this ValidationResult validationResult)
    {
        var validationErrors = validationResult.Errors;

        var errors = from validationError in validationErrors
            let errorMessage = validationError.ErrorMessage
            let error = JsonSerializer.Deserialize<Error>(errorMessage)
            select Error.Validation(error.Code, error.Message, validationError.PropertyName);

        return errors.ToList();
    }
}