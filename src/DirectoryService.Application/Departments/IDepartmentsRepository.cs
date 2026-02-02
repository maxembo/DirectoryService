using CSharpFunctionalExtensions;
using DirectoryService.Domain.Departments;
using Shared;
using Path = DirectoryService.Domain.Departments.Path;

namespace DirectoryService.Application.Departments;

public interface IDepartmentsRepository
{
    Task<Result<Guid, Error>> AddAsync(Department department, CancellationToken cancellationToken = default);

    Task<Result<Department, Error>> GetByIdAsync(
        DepartmentId departmentId, CancellationToken cancellationToken = default);

    Task<UnitResult<Errors>> CheckExistingAndActiveAsync(Guid[] ids, CancellationToken cancellationToken = default);

    Task<UnitResult<Error>> DeleteLocationsAsync(
        DepartmentId departmentId, CancellationToken cancellationToken = default);

    Task<Result<Department, Error>> GetByIdWithLock(DepartmentId id, CancellationToken cancellationToken = default);

    Task LockDescendants(Path path, CancellationToken cancellationToken = default);

    Task<UnitResult<Error>> MoveDepartments(
        DepartmentId parentId,
        Path parentPath,
        Path departmentPath,
        CancellationToken cancellationToken = default);

    Task<UnitResult<Error>> MoveDepartments(Path departmentPath, CancellationToken cancellationToken = default);

    Task<UnitResult<Error>> CheckParentIsChild(
        Path parentPath, Path departmentPath, CancellationToken cancellationToken = default);
}