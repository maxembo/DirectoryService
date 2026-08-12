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
}