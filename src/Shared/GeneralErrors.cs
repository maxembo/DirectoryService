namespace Shared;

public static class GeneralErrors
{
    public static Error ValueIsInvalid(string? name = null)
    {
        string label = name ?? "значение";
        return Error.Validation("value.is.invalid", $"{label} не действительно", name);
    }

    public static Error NotFound(Guid? id = null, string? name = null)
    {
        string forId = id == null ? string.Empty : $"по id {id}";
        return Error.NotFound("value.not.found", $"{name ?? "запись"} не найдена {forId}");
    }

    public static Error ValueIsRequired(string? name = null)
    {
        string label = name ?? string.Empty;
        return Error.Validation("length.is.invalid", $"Поле {label} обязательно", name);
    }

    public static Error AlreadyExist()
    {
        return Error.Conflict("record.already.exist", "Запись уже существует");
    }

    public static Error Failure()
    {
        return Error.Failure("server.failure", "Серверная ошибка");
    }
}