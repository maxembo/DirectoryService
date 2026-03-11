namespace DirectoryService.Application.Constants;

public static class CacheKeys
{
    public const string DEPARTMENT_KEY = "departments_";

    public static string CreateDepartmentsKey(params string[] parameters)
        => DEPARTMENT_KEY + string.Join("_", parameters);
}