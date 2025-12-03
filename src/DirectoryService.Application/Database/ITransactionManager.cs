using CSharpFunctionalExtensions;
using Shared;

namespace DirectoryService.Application.Database;

public interface ITransactionManager
{
    Task<Result<ITransactionScope, Error>> BeginTransactionAsync(CancellationToken cancellationToken = default);

    Task<UnitResult<Error>> SaveChangeAsync(CancellationToken cancellationToken = default);
}