using CSharpFunctionalExtensions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Positions;
using Shared;

namespace DirectoryService.Application.Positions;

public interface IPositionsRepository
{
    Task<Result<Guid, Error>> AddAsync(Position position, CancellationToken cancellationToken = default);

    Task<UnitResult<Error>> DeleteUnusedPositionsByDepartmentIdAsync(
        DepartmentId departmentId, CancellationToken cancellationToken = default);
}