namespace Shared;

public static class GeneralErrors
{
    public static Error Invalid(string? name = null)
    {
        string label = name ?? "значение";
        return Error.Validation("value.is.invalid", $"{label} содержит недопустимое значение", name);
    }

    public static Error NotFound(Guid? id = null, string? name = null)
    {
        string forId = id == null ? string.Empty : $"по id {id}";
        return Error.NotFound("value.not.found", $"{name ?? "запись"} не найдена {forId}");
    }

    public static Error Required(string? name = null)
    {
        string label = name ?? string.Empty;
        return Error.Validation("length.is.invalid", $"{label} обязателен", name);
    }

    public static Error LengthOutOfRange(string? name, int maxLength, int minLength = 0)
    {
        return Error.Validation(
            "value.length.out.of.range", $"{name ?? string.Empty} должно быть от {minLength} до {maxLength} символов");
    }

    public static Error AlreadyExist(string? name = null)
    {
        string label = name ?? string.Empty;
        return Error.Conflict("value.already.exist", $"{name} уже существует");
    }

    public static Error Failure()
    {
        return Error.Failure("server.failure", "Серверная ошибка");
    }

    public static Error MismatchRegex(string? name = null)
    {
        string label = name ?? string.Empty;
        return Error.Validation("value.mismatch.regex", $"{label} имеет недопустимый формат", name);
    }

    public static Error Database(string? code, string? message = null)
    {
        return Error.Failure(code, message ?? "Произошла ошибка в базе данных.");
    }

    public static Error ArrayContainsDuplicates(string? code, string? message = null)
    {
        return Error.Validation(code, message ?? "Массив содержит повторяющиеся значения.");
    }
}