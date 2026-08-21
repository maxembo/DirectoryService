using SharedService.SharedKernel;

namespace DirectoryService.Application.Departments;

public static class DepartmentErrors
{
    public static Error ParentIsArchived()
    {
        return Error.Conflict(
            "department.parent.is.archived",
            "Невозможно восстановить подразделение, пока родительское подразделение находится в архиве.",
            "parentId");
    }

    public static Error MoveWouldCreateCycle()
    {
        return Error.Conflict(
            "department.move.cycle",
            "Нельзя перенести подразделение в самого себя или в его дочернее подразделение.",
            "department.parentId");
    }

    public static Error MoveParentIsArchived()
    {
        return Error.Conflict(
            "department.move.parent_deleted",
            "Нельзя перенести подразделение в архивного родителя.",
            "department.parentId");
    }

    public static Error MoveParentNotFound(Guid parentId)
    {
        return new Error(
            "department.move.parent_not_found",
            $"Родительское подразделение не найдено по id {parentId}.",
            ErrorType.NOT_FOUND,
            "department.parentId");
    }
}