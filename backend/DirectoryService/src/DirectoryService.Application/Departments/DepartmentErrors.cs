using SharedService.SharedKernel;

namespace DirectoryService.Application.Departments;

public static class DepartmentErrors
{
    public static Error ArchivedActivityCannotBeChanged()
    {
        return Error.Conflict(
            "department.activity.archived",
            "Нельзя изменить активность подразделения, находящегося в архиве.",
            "departmentId");
    }

    public static Error ActiveDescendantsPreventDeactivation()
    {
        return Error.Conflict(
            "department.activity.active_descendants",
            "Нельзя деактивировать подразделение, пока у него есть активные дочерние подразделения.",
            "departmentId");
    }

    public static Error InactiveParentPreventsActivation()
    {
        return Error.Conflict(
            "department.activity.inactive_parent",
            "Сначала активируйте родительское подразделение.",
            "departmentId");
    }

    public static Error DepartmentIsNotArchived()
    {
        return Error.Conflict(
            "department.restore.not_archived",
            "Подразделение не находится в архиве.",
            "departmentId");
    }

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
