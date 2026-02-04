using CSharpFunctionalExtensions;

namespace Shared.Database;

public interface ITransactionManager
{
    Task<Result<ITransactionScope, Error>> BeginTransactionAsync(CancellationToken cancellationToken = default);

    Task<UnitResult<Error>> SaveChangeAsync(CancellationToken cancellationToken = default);
}