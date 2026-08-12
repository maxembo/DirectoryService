using CSharpFunctionalExtensions;
using SharedService.SharedKernel;

namespace DirectoryService.Application.Cleanup;

public interface IDeletedEntitiesCleanupService
{
    Task<UnitResult<Error>> Process(CancellationToken cancelToken = default);
}