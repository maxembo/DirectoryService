using CSharpFunctionalExtensions;
using DirectoryService.Domain.Departments;
using Shared;

namespace DirectoryService.Application.Abstractions;

public interface IDepartmentsRepository
{
    Task<Result<Guid, Error>> AddAsync(Department department, CancellationToken cancellationToken = default);

    Task<Result<Department, Error>> GetByIdAsync(Guid departmentId, CancellationToken cancellationToken = default);

    Task<UnitResult<Errors>> CheckExistingAndActiveAsync(Guid[] ids, CancellationToken cancellationToken = default);
}